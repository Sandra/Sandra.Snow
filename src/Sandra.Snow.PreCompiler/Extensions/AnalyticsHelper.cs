namespace Sandra.Snow.PreCompiler.Extensions
{
    using Nancy.ViewEngines.Razor;

    public static class AnalyticsHelper
    {
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

        public static IHtmlString RenderGoogleAnalytics<T>(this HtmlHelpers<T> html, string trackingCode )
        {
            return html.Raw(string.Format(GoogleAnalyticsFormat,  trackingCode));
        }
    }
}
