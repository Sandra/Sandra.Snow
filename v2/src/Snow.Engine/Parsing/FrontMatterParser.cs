using System;
using System.Collections.Generic;
using System.Linq;
using Snow.Engine.Models;

namespace Snow.Engine.Parsing;

/// <summary>
/// Parses YAML-like front matter from markdown files.
/// </summary>
public static class FrontMatterParser
{
    private static readonly Dictionary<string, Func<List<string>, object>> SubSettingParsers = 
        new Dictionary<string, Func<List<string>, object>>
        {
            { "series", ParseSeries }
        };

    /// <summary>
    /// Parses front matter settings from a raw header string.
    /// </summary>
    /// <param name="rawSettings">The raw front matter header content.</param>
    /// <returns>A dictionary of parsed settings.</returns>
    public static Dictionary<string, object> ParseSettings(string? rawSettings)
    {
        if (string.IsNullOrWhiteSpace(rawSettings))
        {
            return new Dictionary<string, object>();
        }

        // Remove the --- delimiters
        rawSettings = rawSettings.Trim('-').Trim();

        var lines = rawSettings.Split(new[] { "\n", "\r", "\n\r" }, StringSplitOptions.RemoveEmptyEntries);
        var result = new Dictionary<string, object>();

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            
            // Skip empty lines
            if (string.IsNullOrEmpty(line))
                continue;

            var setting = line.Split(new[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);

            if (setting.Length == 1)
            {
                // This most likely means that the setting has sub-settings
                var counter = i + 1;
                var subLines = new List<string>();
                var settingName = setting[0].Trim().ToLowerInvariant();

                // Collect all indented sub-lines
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

                // Parse sub-settings if we have a parser for this setting
                if (SubSettingParsers.ContainsKey(settingName))
                {
                    var subSettings = SubSettingParsers[settingName].Invoke(subLines);
                    result.Add(settingName, subSettings);
                }

                i = counter - 1;
            }
            else
            {
                // Simple key-value pair
                var key = setting[0].Trim().ToLowerInvariant();
                var value = setting[1].Trim();

                // Remove quotes if present
                if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                    (value.StartsWith("'") && value.EndsWith("'")))
                {
                    value = value.Substring(1, value.Length - 2);
                }

                result.Add(key, value);
            }
        }

        return result;
    }

    /// <summary>
    /// Parses series information from sub-lines.
    /// </summary>
    /// <param name="subLines">The indented sub-lines containing series data.</param>
    /// <returns>A Series object.</returns>
    private static object ParseSeries(List<string> subLines)
    {
        var seriesResult = new Series();
        var partCount = 1;

        foreach (var subLine in subLines)
        {
            var lineSplit = subLine.Split(new[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (lineSplit.Length != 2)
                continue;

            var key = lineSplit[0].Trim().ToLowerInvariant();
            var value = lineSplit[1].Trim();

            switch (key)
            {
                case "name":
                    var name = value;
                    // Remove quotes if present
                    if ((name.StartsWith("\"") && name.EndsWith("\"")) ||
                        (name.StartsWith("'") && name.EndsWith("'")))
                    {
                        name = name.Substring(1, name.Length - 2);
                    }
                    seriesResult.Name = name;
                    break;
                case "current":
                    if (int.TryParse(value, out var current))
                        seriesResult.Current = current;
                    break;
                case "part":
                    seriesResult.MutableParts.Add(partCount, new SeriesPart(value));
                    partCount++;
                    break;
            }
        }

        return seriesResult;
    }
}
