namespace Snow
{
    using Enums;
    using Exceptions;
    using Extensions;
    using Models;
    using Nancy;
    using Nancy.Testing;
    using Nancy.ViewEngines.Razor;
    using Nancy.ViewEngines.SuperSimpleViewEngine;
    using Newtonsoft.Json;
    using StaticFileProcessors;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    public class Program
    {
        private static void Main(string[] args)
        {
            StaticConfiguration.DisableErrorTraces = false;

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
                                 .Where(x => x.Published != Published.Private)
                                 .ToList();

                posts.SetPostUrl(settings);
                posts.UpdatePartsToLatestInSeries();

                TestModule.Posts = posts;
                TestModule.Drafts = posts.Where(x => x.Published == Published.Draft).ToList();
                TestModule.Categories = CategoriesPage.Create(posts);
                TestModule.PostsGroupedByYearThenMonth = ArchivePage.Create(posts);
                TestModule.MonthYear = ArchiveMenu.Create(posts);
                TestModule.Settings = settings;

                var browserComposer = new Browser(with =>
                {
                    with.Module<TestModule>();
                    with.RootPathProvider<StaticPathProvider>();
                    with.ViewEngines(typeof(SuperSimpleViewEngineWrapper), typeof(RazorViewEngine));
                });

                // Compile all Posts
                posts.ForEach(x => ComposeParsedFiles(x, settings.Output, browserComposer));

                // Compile all Drafts
                var drafts = posts.Where(x => x.Published == Published.Draft).ToList();
                drafts.ForEach(x => ComposeDrafts(x, settings.Output, browserComposer));

                // Compile all static files
                foreach (var processFile in settings.ProcessFiles)
                {
                    var success =
                        ProcessFile(processFile, settings, posts, browserComposer);

                    if (!success)
                    {
                        break;
                    }
                }

                foreach (var copyDirectory in settings.CopyDirectories)
                {
                    var sourceDir = copyDirectory;
                    var destinationDir = copyDirectory;

                    if (copyDirectory.Contains(" => "))
                    {
                        var directorySplit = copyDirectory.Split(new[] { " => " }, StringSplitOptions.RemoveEmptyEntries);

                        sourceDir = directorySplit[0];
                        destinationDir = directorySplit[1];
                    }

                    var source = Path.Combine(settings.CurrentDir, sourceDir);
                    var destination = Path.Combine(settings.Output, destinationDir);
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

        private static bool ProcessFile(StaticFile staticFile, SnowSettings settings, IList<Post> parsedFiles, Browser browserComposer)
        {
            try
            {
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
                Console.WriteLine();
                Console.WriteLine("Error processing static file: ");
                Console.WriteLine();
                Console.WriteLine("- Loop: " + staticFile.Loop);
                Console.WriteLine("- File: " + staticFile.File);
                Console.WriteLine("- Message:");
                Console.WriteLine();
                Console.WriteLine(exception.Message);

                return false;
            }

            return true;
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
                var siteUrl = TestModule.Settings.SiteUrl;

                TestModule.Data = post;
                TestModule.GeneratedUrl = siteUrl + post.Url;

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
            catch (Exception)
            {
            }
        }

        private static void ComposeDrafts(Post post, string output, Browser browserComposer)
        {
            try
            {
                var siteUrl = TestModule.Settings.SiteUrl;

                TestModule.Data = post;
                TestModule.GeneratedUrl = siteUrl + post.Url;

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
            catch (Exception)
            {
            }
        }
    }
}