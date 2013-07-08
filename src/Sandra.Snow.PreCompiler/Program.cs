using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CsQuery.ExtensionMethods;
using Nancy.Helpers;
using Nancy.Testing;
using Nancy.ViewEngines.SuperSimpleViewEngine;
using Newtonsoft.Json;

namespace Sandra.Snow.PreCompiler
{
    using Nancy.ViewEngines.Razor;

    internal class Program
    {
        private static readonly Regex FileNameRegex =
            new Regex(@"^(?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2})-(?<slug>.+).md$",
                      RegexOptions.Compiled | RegexOptions.IgnoreCase);

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

                var parsedFiles = files.Select(x => GetFileData(x, browserParser))
                                       .OrderByDescending(x => x.Date)
                                       .ToList();

                TestModule.PostsGroupedByYearThenMonth = GroupStuff(parsedFiles);

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
                throw;
            }

            Console.WriteLine("Sandra.Snow : " + DateTime.Now.ToShortTimeString() + " : Finish processing");
            Console.ReadKey();
        }

        private static Dictionary<int, Dictionary<int, List<Post>>> GroupStuff(IEnumerable<FileData> parsedFiles)
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

        private static void ProcessStaticFiles(StaticFile staticFile, SnowSettings settings, IList<FileData> parsedFiles,
                                               Browser browserComposer)
        {
            try
            {
                TestModule.StaticFile = staticFile;

                var property = staticFile.Property ?? "";

                switch (property.ToLower())
                {
                    case "postspaged":
                    {
                        const int pageSize = 10;
                        var skip = 0;
                        var iteration = 1;
                        var currentIteration = parsedFiles.Skip(skip).Take(pageSize).ToList();
                        var totalPages = (int) Math.Ceiling((double) parsedFiles.Count()/pageSize);

                        TestModule.TotalPages = totalPages;

                        while (currentIteration.Any())
                        {
                            TestModule.PostsPaged = currentIteration.Select(x => x.Post).ToList();
                            TestModule.PageNumber = iteration;
                            TestModule.HasNextPage = iteration < totalPages;
                            TestModule.HasPreviousPage = iteration > 2 && totalPages > 1;

                            var result = browserComposer.Post("/static");

                            var folder = skip == 0 ? "" : "page" + iteration;
                            var outputFolder = Path.Combine(settings.Output, folder);

                            if (!Directory.Exists(outputFolder))
                            {
                                Directory.CreateDirectory(outputFolder);
                            }

                            File.WriteAllText(Path.Combine(outputFolder, "index.html"), result.Body.AsString());

                            skip += pageSize;
                            iteration++;
                            currentIteration = parsedFiles.Skip(skip).Take(pageSize).ToList();
                        }

                        break;
                    }
                    case "categories":
                    {
                        if (staticFile.Mode == ModeEnum.Each)
                        {
                            foreach (var tempCategory in TestModule.Categories)
                            {
                                var category = tempCategory;

                                var posts = parsedFiles.Select(x => x.Post).Where(x => x.Categories.Contains(category.Name));

                                TestModule.CategoriesInPost = posts.ToList();

                                //TestModule.Data = fileData;
                                var result = browserComposer.Post("/static");

                                var outputFolder = Path.Combine(settings.Output, "category", category.Url);

                                if (!Directory.Exists(outputFolder))
                                {
                                    Directory.CreateDirectory(outputFolder);
                                }

                                File.WriteAllText(Path.Combine(outputFolder, "index.html"), result.Body.AsString());
                            }
                        }
                        else if (staticFile.Mode == ModeEnum.Single)
                        {
                            var result = browserComposer.Post("/static");

                            var outputFolder = Path.Combine(settings.Output, "category");

                            if (!Directory.Exists(outputFolder))
                            {
                                Directory.CreateDirectory(outputFolder);
                            }

                            File.WriteAllText(Path.Combine(outputFolder, "index.html"), result.Body.AsString());
                        }

                        break;
                    }
                    default:
                    {
                        if (staticFile.Mode == ModeEnum.Single)
                        {
                            var result = browserComposer.Post("/static");

                            var outputFolder = Path.Combine(settings.Output, staticFile.File.Substring(0, staticFile.File.IndexOf('.')));

                            if (!Directory.Exists(outputFolder))
                            {
                                Directory.CreateDirectory(outputFolder);
                            }

                            File.WriteAllText(Path.Combine(outputFolder, "index.html"), result.Body.AsString());
                        }

                        break;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
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

        private static void ComposeParsedFiles(FileData fileData, string output, Browser browserComposer)
        {
            try
            {
                TestModule.Data = fileData;
                var result = browserComposer.Post("/compose");

                var outputFolder = Path.Combine(output, fileData.Year.ToString(), fileData.Month.ToString(), fileData.Slug);

                if (!Directory.Exists(outputFolder))
                {
                    Directory.CreateDirectory(outputFolder);
                }

                File.WriteAllText(Path.Combine(outputFolder, "index.html"), result.Body.AsString());
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private static FileData GetFileData(FileInfo file, Browser browser)
        {
            var response = browser.Get("/post/" + HttpUtility.UrlEncodeUnicode(file.Name));
            var rawPost = File.ReadAllText(file.FullName);
            var fileNameMatches = FileNameRegex.Match(file.Name);
            var settings = new Dictionary<string, dynamic>();
            var rawSettings = string.Empty;

            if (!fileNameMatches.Success)
            {
                throw new ApplicationException("File " + file.Name +
                                               " does not match the format {year}-{month}-{day}-{slug}.(md|markdown)");
            }

            var startOfSettingsIndex = rawPost.IndexOf("---", StringComparison.InvariantCultureIgnoreCase);
            if (startOfSettingsIndex >= 0)
            {
                int endOfSettingsIndex = rawPost.IndexOf("---", startOfSettingsIndex + 3,
                                                         StringComparison.InvariantCultureIgnoreCase);

                rawSettings = rawPost.Substring(startOfSettingsIndex + 3, endOfSettingsIndex - 3);

                settings = ParseSettings(rawSettings);
            }

            //var realContent = rawPost.Substring(endOfSettingsIndex, rawPost.Length - endOfSettingsIndex);
            //var renderedContent = (new Markdown
            //{
            //    ExtraMode = true
            //}).Transform(realContent);

            var year = fileNameMatches.Groups["year"].Value;
            var month = fileNameMatches.Groups["month"].Value;
            var day = fileNameMatches.Groups["day"].Value;
            var slug = fileNameMatches.Groups["slug"].Value;
            var date = DateTime.ParseExact(year + month + day, "yyyyMMdd", CultureInfo.InvariantCulture);

            return new FileData(settings)
            {
                FileName = file.Name,
                RawSettings = rawSettings,
                Content = response.Body.AsString(),
                Settings = settings,
                Year = date.Year,
                Month = date.Month,
                Day = date.Day,
                Date = date,
                Slug = slug
            };
        }

        private static Dictionary<string, object> ParseSettings(string rawSettings)
        {
            var lines = rawSettings.Split(new[] { "\n", "\r", "\n\r" }, StringSplitOptions.RemoveEmptyEntries);
            var result = new Dictionary<string, object>();

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                var setting = line.Split(':');

                if (setting[1].Trim() == string.Empty)
                {
                    //This most likely means that the setting has sub-settings
                    var subSettings = new Dictionary<string, string>();
                    var counter = i + 1;

                    for (; counter < lines.Length; counter++)
                    {
                        var subLine = lines[counter];
                        var subLineSetting = subLine.Split(':');

                        if (char.IsWhiteSpace(subLine, 0))
                        {
                            subSettings.Add(subLineSetting[0].Trim(), subLineSetting[1].Trim());
                            continue;
                        }

                        break;
                    }

                    result.Add(setting[0], subSettings);
                    i = counter - 1;
                }
                else
                {
                    result.Add(setting[0].Trim(), setting[1].Trim());
                }
            }

            return result;
        }
    }
}