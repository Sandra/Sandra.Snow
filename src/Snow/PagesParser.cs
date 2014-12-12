namespace Snow
{
  using System;
  using System.Globalization;
  using System.IO;
  using System.Text.RegularExpressions;
  using Extensions;
  using MarkdownSharp;
  using Models;
  using PostParsers;

  public class PagesParser
  {
    private static readonly Regex FileNameRegex =
        new Regex(@"(?<slug>.+).(?:md|markdown)$",
                  RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static Page GetFileData(FileInfo file, SnowSettings snowSettings)
    {
      var rawPage = File.ReadAllText(file.FullName);
      var fileNameMatches = FileNameRegex.Match(file.Name);
      var rawSettings = string.Empty;

      if (!fileNameMatches.Success)
      {
        file.Name.OutputIfDebug(" - Skipping file: ");
        " - File does not match the format {slug}.(md|markdown)".OutputIfDebug();
        return null;
      }

      var result = MarkdownFileParser.ParseDataFromFile(rawPage);
      var settings = PostParser.ParseSettings(result.Header);

      var slug = fileNameMatches.Groups["slug"].Value.ToUrlSlug();

      /// if a 'date' property is found in markdown file header, that date will be used instead of the date in the file name
      DateTime date = DateTime.Now;
      if (settings.ContainsKey("date"))
          DateTime.TryParse((string)settings["date"], out date);

      var markdown = new Markdown();
      var bodySerialized = markdown.Transform(result.Body);
      var excerptSerialized = markdown.Transform(result.Excerpt ?? string.Empty);

      var pageHeader = new Page
      {
        FileName = file.Name,
        MarkdownHeader = rawSettings,
        Content = bodySerialized,
        ContentExcerpt = excerptSerialized,
        Settings = settings,
        Date = date,
        Url = "/" + slug
      };

      pageHeader.SetSnowSettings(snowSettings);
      pageHeader.SetHeaderSettings(settings);

      return pageHeader;
    }
  }
}
