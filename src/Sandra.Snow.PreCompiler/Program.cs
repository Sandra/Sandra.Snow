namespace Sandra.Snow.PreCompiler
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using CsQuery.ExtensionMethods;
    using Nancy;
    using Nancy.Testing;
    using Nancy.ViewEngines.Razor;
    using Nancy.ViewEngines.SuperSimpleViewEngine;
    using Newtonsoft.Json;
    using Sandra.Snow.PreCompiler.Exceptions;
    using Sandra.Snow.PreCompiler.Extensions;
    using Sandra.Snow.PreCompiler.Models;
    using Sandra.Snow.PreCompiler.StaticFileProcessors;
    using Sandra.Snow.PreCompiler.ViewModels;

    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Sandra.Snow : " + DateTime.Now.ToShortTimeString() + " : Begin processing");

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

                var parsedFiles = files.Select(x => PostSettingsParser.GetFileData(x, browserParser, settings))
                                       .OrderByDescending(x => x.Date)
                                       .ToList();

                TestModule.PostsGroupedByYearThenMonth = GroupStuff(parsedFiles);
                TestModule.MonthYear = GroupMonthYearArchive(parsedFiles);

                var browserComposer = new Browser(with =>
                {
                    with.Module<TestModule>();
                    with.RootPathProvider<StaticPathProvider>();
                    with.ViewEngines(typeof(SuperSimpleViewEngineWrapper), typeof(RazorViewEngine));
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("Sandra.Snow : " + DateTime.Now.ToShortTimeString() + " : Finish processing");
            Console.ReadKey();
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
                                                                                                 u =>
                                                                                                 u.Count()));

            return (from s in groupedByYear
                    from y in s.Value
                    select new BaseViewModel.MonthYear
                    {
                        Count = y.Value,
                        Title = y.Key.ToString("MMMM, yyyy"),
                        Url = "/archive#" + y.Key.ToString("yyyyMMMM")
                    }).ToList();
        }

        private static Dictionary<int, Dictionary<int, List<Post>>> GroupStuff(IEnumerable<PostHeaderSettings> parsedFiles)
        {
            var groupedByYear = (from p in parsedFiles
                                    group p by p.Year
                                    into g
                                    select g).ToDictionary(x => x.Key, x => (from y in x
                                                                            group y by y.Month
                                                                            into p
                                                                            select p).ToDictionary(u => u.Key,
                                                                                                    u =>
                                                                                                    u.Select(p => p.Post).ToList()));

            return groupedByYear;
        }

        private static void ProcessStaticFiles(StaticFile staticFile, SnowSettings settings, IList<PostHeaderSettings> parsedFiles,
                                               Browser browserComposer)
        {
            try
            {
                TestModule.StaticFile = staticFile;
                
                var property = staticFile.Property ?? "";

                var processor = ProcessorFactory.Get(property.ToLower(), staticFile.Mode);

                if (processor == null)
                {
                    throw new ProcessorNotFoundException(property.ToLower());
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
                Console.WriteLine("- " + staticFile.Property);
                Console.WriteLine("- " + staticFile.File);
                Console.WriteLine("- " + staticFile.Mode);
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

        private static void ComposeParsedFiles(PostHeaderSettings postHeaderSettings, string output, Browser browserComposer)
        {
            try
            {
                TestModule.Data = postHeaderSettings;
                var result = browserComposer.Post("/compose");

                if (result.StatusCode != HttpStatusCode.OK)
                {
                    throw new FileProcessingException("Processing failed composing " + postHeaderSettings.FileName);
                }

                var outputFolder = Path.Combine(output, postHeaderSettings.Year.ToString(CultureInfo.InvariantCulture), postHeaderSettings.Date.ToString("MM"), postHeaderSettings.Slug);

                if (!Directory.Exists(outputFolder))
                {
                    Directory.CreateDirectory(outputFolder);
                }

                File.WriteAllText(Path.Combine(outputFolder, "index.html"), result.Body.AsString());
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
        }
    }
}