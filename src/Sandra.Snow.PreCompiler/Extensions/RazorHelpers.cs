﻿namespace Sandra.Snow.PreCompiler.Extensions
{
    using System;
    using System.Linq;
    using System.Text;
    using Models;
    using Nancy.ViewEngines.Razor;
    using ViewModels;

    public static class RazorHelpers
    {
        private const string ImageFormat = @"<img src=""{0}"" />";

        private const string GoogleAnalyticsFormat = @"<script type=""text/javascript"">
var _gaq = _gaq || [];

_gaq.push(['_setAccount', '{0}']);
_gaq.push(['_trackPageview']);
        
(function () {{
    var ga = document.createElement('script');
    ga.type = 'text/javascript';
    ga.async = true;
    ga.src = ('https:' == document.location.protocol ? 'https://ssl' : 'http://www') + '.google-analytics.com/ga.js';
    var s = document.getElementsByTagName('script')[0];
    s.parentNode.insertBefore(ga, s);
}})();
</script>";

        private const string DisqusFormat = @"<div id=""disqus_thread""></div>
<script>
    var disqus_shortname = '{0}';
    var disqus_url = '{1}';

    (function() {{
        var dsq = document.createElement('script'); dsq.type = 'text/javascript'; dsq.async = true;
        dsq.src = '//' + disqus_shortname + '.disqus.com/embed.js';
        (document.getElementsByTagName('head')[0] || document.getElementsByTagName('body')[0]).appendChild(dsq);
    }})();
</script>
<noscript>Please enable JavaScript to view the <a href=""http://disqus.com/?ref_noscript"">comments powered by Disqus.</a></noscript>
<a href=""http://disqus.com"" class=""dsq-brlink"">comments powered by <span class=""logo-disqus"">Disqus</span></a>";

        public static IHtmlString RenderGravatarImage<T>(this HtmlHelpers<T> html, string email, int size = 0)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return html.Raw("");
            }

            var url = email.EmailToGravatar(size);
            var result = string.Format(ImageFormat, url);

            return html.Raw(result);
        }
        
        public static IHtmlString RenderDisqusComments<T>(this HtmlHelpers<T> html, string shortName)
        {
            var postViewModel = html.Model as PostViewModel;

            if (postViewModel != null)
            {
                if (string.IsNullOrWhiteSpace(postViewModel.Settings.SiteUrl))
                {
                    throw new ArgumentException("Snow Settings requires a SiteUrl to be set to embed Disqus");
                }

                return html.Raw(string.Format(DisqusFormat, shortName, postViewModel.Settings.SiteUrl + postViewModel.Url));
            }

            return html.Raw("");
        }

        public static IHtmlString RenderSeries(this HtmlHelpers<PostViewModel> html, string className = "snow-series")
        {
            return RenderSeries(html, html.Model.Series, className);
        }

        public static IHtmlString RenderSeries(this HtmlHelpers<ContentViewModel> html, Post post, string className = "snow-series")
        {
            return RenderSeries(html, post.Series, className);
        }

        private static IHtmlString RenderSeries<T>(HtmlHelpers<T> html, Series series, string className)
        {
            if (series == null || !series.Parts.Any())
            {
                return html.Raw("");
            }

            var result = new StringBuilder();

            result.AppendFormat(@"<ul class=""{0}"">", className);

            foreach (var part in series.Parts)
            {
                if (part.Value.HasUrl() && part.Key != series.Current)
                {
                    result.AppendFormat(@"<li><a href=""{0}"">{1}</a></li>", part.Value.Url, part.Value.Name);
                }
                else
                {
                    result.AppendFormat(@"<li>{0}</li>", part.Value.Name);
                }
            }

            result.Append("</ul>");

            return html.Raw(result.ToString());
        }

        public static IHtmlString RenderGoogleAnalytics<T>(this HtmlHelpers<T> html, string trackingCode)
        {
            return html.Raw(string.Format(GoogleAnalyticsFormat, trackingCode));
        }
    }
}