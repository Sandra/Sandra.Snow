namespace Sandra.Snow.PreCompiler
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using CsQuery.ExtensionMethods;
    using Exceptions;
    using Extensions;
    using Models;
    using Nancy.Testing;
    using Nancy.ViewEngines.Razor;
    using Nancy.ViewEngines.SuperSimpleViewEngine;
    using Newtonsoft.Json;
    using StaticFileProcessors;
    using ViewModels;

    public class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Sandra.Snow : " + DateTime.Now.ToString("HH:mm:ss") + " : Begin processing");

            try
            {
                var commands = args.Select(x => x.Split('=')).ToDictionary(x => x[0], x => x[1]);

                if (commands.ContainsKey("debug"))
                {
                    DebugHelperExtensions.EnableDebugging();
                }

                string currentDir;

                if (commands.ContainsKey("config"))
                {
                    currentDir = new FileInfo(commands["config"]).DirectoryName;
                }
                else
                {
                    currentDir = Path.GetDirectoryName(typeof(Program).Assembly.Location);
                }

                currentDir.OutputIfDebug(prefixWith: "current directory: ");

                var settings = CreateSettings(currentDir);

                var extensions = new HashSet<string>(new[] { ".md", ".markdown" }, StringComparer.OrdinalIgnoreCase);
                var files = new DirectoryInfo(settings.Posts).EnumerateFiles()
                                                             .Where(x => extensions.Contains(x.Extension));

                SetupOutput(settings);

                StaticPathProvider.Path = settings.CurrentDir;
                SnowViewLocationConventions.Settings = settings;

                var browserParser = new Browser(with =>
                {
                    with.Module<TestModule>();
                    with.RootPathProvider<StaticPathProvider>();
                    with.ViewEngine<CustomMarkDownViewEngine>();
                });

                var posts = files.Select(x => PostParser.GetFileData(x, browserParser, settings))
                                 .OrderByDescending(x => x.Date)
                                 .ToList();

                posts.SetPostUrl(settings);
                posts.UpdatePartsToLatestInSeries();

                var categories = (from c in posts.SelectMany(x => x.Categories)
                                  group c by c
                                  into g
                                  select new Category
                                  {
                                      Name = g.Key,
                                      Count = g.Count()
                                  }).OrderBy(cat => cat.Name).ToList();

                TestModule.Posts = posts;
                TestModule.Categories = categories;
                TestModule.PostsGroupedByYearThenMonth = GroupStuff(posts);
                TestModule.MonthYear = GroupMonthYearArchive(posts);
                TestModule.Settings = settings;

                var browserComposer = new Browser(with =>
                {
                    with.Module<TestModule>();
                    with.RootPathProvider<StaticPathProvider>();
                    with.ViewEngines(typeof(SuperSimpleViewEngineWrapper), typeof(RazorViewEngine));
                });

                // Compile all Posts
                posts.ForEach(x => ComposeParsedFiles(x, settings.Output, browserComposer));

                // Compile all static files
                settings.ProcessFiles.ForEach(x => ProcessFiles(x, settings, posts, browserComposer));

                foreach (var copyDirectory in settings.CopyDirectories)
                {
                    var source = Path.Combine(settings.CurrentDir, copyDirectory);
                    var destination = Path.Combine(settings.Output, copyDirectory);
                    new DirectoryInfo(source).Copy(destination, true);
                }

                if (commands.ContainsKey("debug"))
                {
                    DebugHelperExtensions.WaitForContinue();
                }

                Console.WriteLine("Sandra.Snow : " + DateTime.Now.ToString("HH:mm:ss") + " : Finish processing");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                DebugHelperExtensions.WaitForContinue();
            }
        }

        private static void SetupOutput(SnowSettings settings)
        {
            if (!Directory.Exists(settings.Output))
            {
                Directory.CreateDirectory(settings.Output);
            }

            new DirectoryInfo(settings.Output).Empty();
        }

        private static List<BaseViewModel.MonthYear> GroupMonthYearArchive(IEnumerable<Post> parsedFiles)
        {
            var groupedByYear = (from p in parsedFiles
                                 group p by p.Date.AsYearDate()
                                 into g
                                 select g).ToDictionary(x => x.Key, x => (from y in x
                                                                          group y by y.Date.AsMonthDate()
                                                                          into p
                                                                          select p).ToDictionary(u => u.Key,
                                                                              u => u.Count()));

            return (from s in groupedByYear
                    from y in s.Value
                    select new BaseViewModel.MonthYear
                    {
                        Count = y.Value,
                        Title = y.Key.ToString("MMMM, yyyy"),
                        Url = "/archive#" + y.Key.ToString("yyyyMMMM")
                    }).ToList();
        }

        private static Dictionary<int, Dictionary<int, List<Post>>> GroupStuff(IEnumerable<Post> parsedFiles)
        {
            var groupedByYear = (from p in parsedFiles
                                 group p by p.Year
                                     into g
                                     select g).ToDictionary(x => x.Key, x => (from y in x
                                                                              group y by y.Month
                                                                                  into p
                                                                                  select p).ToDictionary(u => u.Key,
                                                                              u => u.ToList()));

            return groupedByYear;
        }

        private static void ProcessFiles(StaticFile staticFile, SnowSettings settings, IList<Post> parsedFiles, Browser browserComposer)
        {
            try
            {
                TestModule.StaticFile = staticFile;

                var processorName = staticFile.Loop ?? "";
                var processor = ProcessorFactory.Get(processorName.ToLower());

                if (processor == null)
                {
                    throw new ProcessorNotFoundException(processorName.ToLower());
                }

                processor.Process(new SnowyData
                {
                    Settings = settings,
                    Files = parsedFiles,
                    Browser = browserComposer,
                    File = staticFile
                }, settings);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Error processing static file: ");
                Console.WriteLine("- " + staticFile.Loop);
                Console.WriteLine("- " + staticFile.File);
                Console.WriteLine("- Exception:");
                Console.WriteLine(exception);
            }
        }

        private static SnowSettings CreateSettings(string currentDir)
        {
            var settings = SnowSettings.Default(currentDir);
            var configFile = Path.Combine(currentDir, "snow.config");

            if (!File.Exists(configFile))
            {
                throw new FileNotFoundException("Snow config file not found");
            }

            var fileData = File.ReadAllText(configFile);

            var newSettings = JsonConvert.DeserializeObject<SnowSettings>(fileData);

            var properties = newSettings.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var propertyInfo in properties)
            {
                var value = propertyInfo.GetValue(newSettings);

                var singleString = value as string;

                if (!string.IsNullOrWhiteSpace(singleString))
                {
                    propertyInfo.SetValue(settings, value);
                }

                var strings = value as string[];
                if (strings != null && strings.Length > 0)
                {
                    propertyInfo.SetValue(settings, value);
                }

                var staticFiles = value as IEnumerable<StaticFile>;
                if (staticFiles != null && staticFiles.Any())
                {
                    propertyInfo.SetValue(settings, value);
                }
            }

            if (!string.IsNullOrWhiteSpace(settings.SiteUrl))
            {
                settings.SiteUrl = settings.SiteUrl.TrimEnd('/');
            }

            return settings;
        }

        private static void ComposeParsedFiles(Post post, string output, Browser browserComposer)
        {
            try
            {
                TestModule.Data = post;

                var result = browserComposer.Post("/compose");

                result.ThrowIfNotSuccessful(post.FileName);

                var body = result.Body.AsString();

                var outputFolder = Path.Combine(output, post.Url.Trim('/')); //Outputfolder is incorrect with leading slash on urlFormat

                if (!Directory.Exists(outputFolder))
                {
                    Directory.CreateDirectory(outputFolder);
                }

                File.WriteAllText(Path.Combine(outputFolder, "index.html"), body);
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
        }
    }
}