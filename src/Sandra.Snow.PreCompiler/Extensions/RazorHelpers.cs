namespace Sandra.Snow.PreCompiler.Extensions
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

        public static IHtmlString RenderDisqusComments<T>(this HtmlHelpers<T> html, string shortName)
        {
            var postViewModel = html.Model as PostViewModel;
            
            if (postViewModel != null)
            {
                if (string.IsNullOrWhiteSpace(postViewModel.Settings.SiteUrl))
                {
                    throw new ArgumentException("Snow Settings requires a SiteUrl to be set to embed Disqus");
                }

                return html.Raw(string.Format(DisqusFormat, shortName, postViewModel.Settings.SiteUrl +  postViewModel.Url));
            }

            return html.Raw("");
        }

        public static IHtmlString RenderSeries(this HtmlHelpers<PostViewModel> html, string className = "snow-series")
        {
            return RenderSeries(html, html.Model.Series, className);
        }

        //public static IHtmlString RenderSeries(this HtmlHelpers<ContentViewModel> html)
        //{
        //    return RenderSeries(html, html.Model);
        //}

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
    }
}