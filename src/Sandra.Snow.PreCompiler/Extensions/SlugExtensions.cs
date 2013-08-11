namespace Sandra.Snow.PreCompiler.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Text;
    using Models;

    public static class SlugExtensions
    {
        private static readonly SortedList<int, Func<string, DateTime, string, string>> UrlFormatParser = new SortedList
            <int, Func<string, DateTime, string, string>>
        {
            {0, DayFull},
            {1, DayAbbreviated},
            {2, Day},
            {3, MonthFull},
            {4, MonthAbbreviated},
            {5, Month},
            {6, YearFull},
            {7, Year},
            {8, Slug}
        };

        public static string ToUrlSlug(this string value)
        {

            //First to lower case
            value = value.ToLowerInvariant();

            //Remove all accents
            var bytes = Encoding.GetEncoding("Cyrillic").GetBytes(value);
            value = Encoding.ASCII.GetString(bytes);

            //Replace spaces
            value = Regex.Replace(value, @"\s", "-", RegexOptions.Compiled);

            //Remove invalid chars
            value = Regex.Replace(value, @"[^a-z0-9\s-_]", "", RegexOptions.Compiled);

            //Trim dashes from end
            value = value.Trim('-', '_');

            //Replace double occurences of - or _
            value = Regex.Replace(value, @"([-_]){2,}", "$1", RegexOptions.Compiled);

            return value;
        }

        public static void SetPostUrl(this IEnumerable<Post> posts, SnowSettings settings)
        {
            foreach (var postHeader in posts)
            {
                var urlFormat = "/" + settings.UrlFormat.Trim('/') + "/";

                foreach (var s in UrlFormatParser.OrderBy(x => x.Key).Select(x => x.Value))
                {
                    urlFormat = s.Invoke(urlFormat, postHeader.Date, postHeader.Url);
                }

                postHeader.Url = urlFormat;
            }
        }

        private static string DayFull(string url, DateTime replaceDate, string slug)
        {
            return url.Replace("dddd", replaceDate.ToString("dddd"));
        }

        private static string DayAbbreviated(string url, DateTime replaceDate, string slug)
        {
            return url.Replace("ddd", replaceDate.ToString("ddd"));
        }

        private static string Day(string url, DateTime replaceDate, string slug)
        {
            return url.Replace("dd", replaceDate.ToString("dd"));
        }

        private static string Month(string url, DateTime replaceDate, string slug)
        {
            return url.Replace("MM", replaceDate.ToString("MM"));
        }

        private static string MonthAbbreviated(string url, DateTime replaceDate, string slug)
        {
            return url.Replace("MMM", replaceDate.ToString("MMM"));
        }

        private static string MonthFull(string url, DateTime replaceDate, string slug)
        {
            return url.Replace("MMMM", replaceDate.ToString("MMMM"));
        }

        private static string Slug(string url, DateTime replaceDate, string slug)
        {
            return url.Replace("slug", slug);
        }

        private static string YearFull(string url, DateTime replaceDate, string slug)
        {
            return url.Replace("yyyy", replaceDate.ToString("yyyy"));
        }

        private static string Year(string url, DateTime replaceDate, string slug)
        {
            return url.Replace("yy", replaceDate.ToString("yy"));
        }
    }
}