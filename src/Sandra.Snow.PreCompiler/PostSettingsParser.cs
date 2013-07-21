namespace Sandra.Snow.PreCompiler
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text.RegularExpressions;
    using Nancy.Helpers;
    using Nancy.Testing;

    public class PostSettingsParser
    {
        private static readonly Regex FileNameRegex =
            new Regex(@"^(?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2})-(?<slug>.+).md$",
                      RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static FileData GetFileData(FileInfo file, Browser browser)
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

            //Get the Header info from a Post Markdown File
            //Find the first index of ---
            var startOfSettingsIndex = rawPost.IndexOf("---", StringComparison.InvariantCultureIgnoreCase);
            if (startOfSettingsIndex >= 0)
            {
                //Find the second index of --- after the first
                var endOfSettingsIndex = rawPost.IndexOf("---", startOfSettingsIndex + 3,
                                                         StringComparison.InvariantCultureIgnoreCase);

                //If we find the 2nd index, parse the settings
                //Otherwise we assume there's no header or settings...
                if (endOfSettingsIndex >= 0)
                {
                    rawSettings = rawPost.Substring(startOfSettingsIndex + 3, endOfSettingsIndex - 3);
                    settings = ParseSettings(rawSettings);
                }
            }

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

                var setting = line.Split(new[] { ':' }, 2);

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
