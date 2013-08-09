namespace Sandra.Snow.PreCompiler
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
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

    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Sandra.Snow : " + DateTime.Now.ToString("HH:mm:ss") + " : Begin processing");

            try
            {
                var commands = args.Select(x => x.Split('=')).ToDictionary(x => x[0], x => x[1]);

                string currentDir;

                if (commands.ContainsKey("config"))
                {
                    currentDir = new FileInfo(commands["config"]).DirectoryName;
                }
                else
                {
                    currentDir = Path.GetDirectoryName(typeof(Program).Assembly.Location);
                }

                var settings = CreateSettings(currentDir);

                StaticPathProvider.Path = settings.CurrentSnowDir;

                var extensions = new HashSet<string>(new[] { ".md", ".markdown" }, StringComparer.OrdinalIgnoreCase);
                var files = new DirectoryInfo(settings.Posts).EnumerateFiles()
                                                             .Where(x => extensions.Contains(x.Extension));

                if (!Directory.Exists(settings.Output))
                {
                    Directory.CreateDirectory(settings.Output);
                }

                new DirectoryInfo(settings.Output).Empty();

                var browserParser = new Browser(with =>
                {
                    with.Module<TestModule>();
                    with.RootPathProvider<StaticPathProvider>();
                    with.ViewEngine<CustomMarkDownViewEngine>();
                });

                var parsedFiles = files.Select(x => PostParser.GetFileData(x, browserParser, settings))
                                       .OrderByDescending(x => x.Date)
                                       .ToList();

                TestModule.PostsGroupedByYearThenMonth = GroupStuff(parsedFiles);
                TestModule.MonthYear = GroupMonthYearArchive(parsedFiles);
                TestModule.Settings = settings;

                var browserComposer = new Browser(with =>
                {
                    with.Module<TestModule>();
                    with.RootPathProvider<StaticPathProvider>();
                    with.ViewEngines(typeof (SuperSimpleViewEngineWrapper), typeof (RazorViewEngine));
                });

                parsedFiles.ForEach(x => ComposeParsedFiles(x, settings.Output, browserComposer));

                var categories = parsedFiles.SelectMany(x => x.Categories)
                                            .Distinct()
                                            .Select(x => new Category(x));

                TestModule.Categories = categories.ToList();

                settings.ProcessStaticFiles.ForEach(x => ProcessStaticFiles(x, settings, parsedFiles, browserComposer));

                foreach (var copyDirectory in settings.CopyDirectories)
                {
                    var source = Path.Combine(settings.CurrentSnowDir, copyDirectory);
                    var destination = Path.Combine(settings.Output, copyDirectory);
                    new DirectoryInfo(source).Copy(destination, true);
                }

                if (commands.ContainsKey("debug"))
                {
                    Console.WriteLine("Paused - press any key");
                    Console.ReadKey();
                }

                Console.WriteLine("Sandra.Snow : " + DateTime.Now.ToString("HH:mm:ss") + " : Finish processing");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static IList<BaseViewModel.MonthYear> GroupMonthYearArchive(IEnumerable<PostHeaderSettings> parsedFiles)
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

        private static Dictionary<int, Dictionary<int, List<Post>>> GroupStuff(
            IEnumerable<PostHeaderSettings> parsedFiles)
        {
            var groupedByYear = (from p in parsedFiles
                                 group p by p.Year
                                 into g
                                 select g).ToDictionary(x => x.Key, x => (from y in x
                                                                          group y by y.Month
                                                                          into p
                                                                          select p).ToDictionary(u => u.Key,
                                                                              u => u.Select(p => p.Post).ToList()));

            return groupedByYear;
        }

        private static void ProcessStaticFiles(StaticFile staticFile, SnowSettings settings,
            IList<PostHeaderSettings> parsedFiles,
            Browser browserComposer)
        {
            try
            {
                TestModule.StaticFile = staticFile;

                var processorName = staticFile.ProcessorName ?? "";
                var processor = ProcessorFactory.Get(processorName.ToLower(), staticFile.IterateModel);

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
                });
            }
            catch (Exception exception)
            {
                Console.WriteLine("Error processing static file: ");
                Console.WriteLine("- " + staticFile.ProcessorName);
                Console.WriteLine("- " + staticFile.File);
                Console.WriteLine("- " + staticFile.IterateModel);
                Console.WriteLine("- Exception:");
                Console.WriteLine(exception);
            }
        }

        private static SnowSettings CreateSettings(string currentDir)
        {
            var settings = SnowSettings.Default(currentDir);

            if (!File.Exists(Path.Combine(currentDir, "snow.config")))
            {
                throw new FileNotFoundException("Snow config file not found");
            }

            var fileData = File.ReadAllText(currentDir + "/snow.config");

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

            return settings;
        }

        private static void ComposeParsedFiles(PostHeaderSettings postHeaderSettings, string output,
            Browser browserComposer)
        {
            try
            {
                TestModule.Data = postHeaderSettings;
                var result = browserComposer.Post("/compose");
                var body = result.Body.AsString();

                //if (result.StatusCode != HttpStatusCode.OK)
                //Crappy check because Nancy returns 200 on a compilation error :(
                if (body.Contains("<title>Razor Compilation Error</title>") &&
                    body.Contains("<p>We tried, we really did, but we just can't compile your view.</p>"))
                {
                    throw new FileProcessingException("Processing failed composing " + postHeaderSettings.FileName);
                }

                var outputFolder = Path.Combine(output, postHeaderSettings.Year.ToString(CultureInfo.InvariantCulture), postHeaderSettings.Date.ToString("MM"), postHeaderSettings.Slug);

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