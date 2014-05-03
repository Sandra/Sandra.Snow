namespace Snow
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using Extensions;
    using MarkdownSharp;
    using Models;
    using Nancy.Helpers;
    using Nancy.Testing;
    using PostParsers;

    public class PostParser
    {
        private static readonly Regex FileNameRegex =
            new Regex(@"^(?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2})-(?<slug>.+).(?:md|markdown)$",
                      RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly IDictionary<string, Func<List<string>, object>>
            SubSettingParsers = new Dictionary<string, Func<List<string>, object>>();

        private static readonly Markdown Markdown = new Markdown();

        static PostParser()
        {
            SubSettingParsers.Add("series", SeriesParser);
        }

        public static Post GetFileData(FileInfo file, SnowSettings snowSettings)
        {
            var rawPost = File.ReadAllText(file.FullName);
            var fileNameMatches = FileNameRegex.Match(file.Name);
            var rawSettings = string.Empty;

            if (!fileNameMatches.Success)
            {
                file.Name.OutputIfDebug(" - Skipping file: ");
                " - File does not match the format {year}-{month}-{day}-{slug}.(md|markdown)".OutputIfDebug();
                return new Post.MissingPost();
            }

            var result = MarkdownFileParser.ParseDataFromFile(rawPost);
            var settings = ParseSettings(result.Header);

            var year = fileNameMatches.Groups["year"].Value;
            var month = fileNameMatches.Groups["month"].Value;
            var day = fileNameMatches.Groups["day"].Value;
            var slug = fileNameMatches.Groups["slug"].Value.ToUrlSlug();
            var date = DateTime.ParseExact(year + month + day, "yyyyMMdd", CultureInfo.InvariantCulture);

            /// if a 'date' property is found in markdown file header, that date will be used instead of the date in the file name
            if (settings.ContainsKey("date"))
            {
                try
                {
                    date = DateTime.Parse((string)settings["date"]);
                }
                finally
                {
                    /// do nothing, let the current 'date' be as is
                }
            }

            var bodySerialized = Markdown.Transform(result.Body);
            var excerptSerialized = Markdown.Transform(result.Excerpt ?? string.Empty);

            var postHeader = new Post
            {
                FileName = file.Name,
                MarkdownHeader = rawSettings,
                Content = bodySerialized,
                ContentExcerpt = excerptSerialized,
                Settings = settings,
                Year = date.Year,
                Month = date.Month,
                Day = date.Day,
                Date = date,
                Url = slug
            };

            postHeader.SetSnowSettings(snowSettings);
            postHeader.SetHeaderSettings(settings);

            return postHeader;
        }
        
        public static Dictionary<string, object> ParseSettings(string rawSettings)
        {
            if (string.IsNullOrWhiteSpace(rawSettings))
            {
                return new Dictionary<string, object>();
            }

            rawSettings = rawSettings.Trim('-');

            var lines = rawSettings.Split(new[] { "\n", "\r", "\n\r" }, StringSplitOptions.RemoveEmptyEntries);
            var result = new Dictionary<string, object>();

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                var setting = line.Split(new[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);

                if (setting.Length == 1)
                {
                    //This most likely means that the setting has sub-settings
                    var counter = i + 1;
                    var subLines = new List<string>();
                    var settingName = setting[0];

                    for (; counter < lines.Length; counter++)
                    {
                        var subLine = lines[counter];

                        if (char.IsWhiteSpace(subLine, 0))
                        {
                            subLines.Add(subLine);
                            continue;
                        }

                        break;
                    }

                    var subSettings = SubSettingParsers[settingName].Invoke(subLines);

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

        private static object SeriesParser(List<string> subLines)
        {
            var seriesResult = new Series();
            var partCount = 1;

            foreach (var lineSplit in subLines.Select(t => t.Split(new[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries)))
            {
                switch (lineSplit[0].Trim())
                {
                    case "name":
                    {
                        seriesResult.Name = lineSplit[1].Trim();
                        break;
                    }
                    case "current":
                    {
                        seriesResult.Current = int.Parse(lineSplit[1].Trim());
                        break;
                    }
                    case "part":
                    {
                        seriesResult.Parts.Add(partCount, new Series.Part
                        {
                            Name = lineSplit[1].Trim()
                        });
                        partCount++;
                        break;
                    }
                }
            }

            return seriesResult;
        }
    }
}