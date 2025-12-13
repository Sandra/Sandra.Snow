using System;
using System.Text;
using Snow.Abstractions;
using Snow.Engine.ViewModels;

namespace Snow.Engine.Templates.Razor;

/// <summary>
/// HTML helpers for Snow Razor templates.
/// Provides Nancy-compatible @Html.* methods that v1 templates used.
/// </summary>
/// <typeparam name="TModel">The model type.</typeparam>
public class SnowHtmlHelpers<TModel>
{
    private readonly SnowTemplateBase<TModel> _page;

    public SnowHtmlHelpers(SnowTemplateBase<TModel> page)
    {
        _page = page;
    }

    /// <summary>
    /// Returns raw HTML without encoding.
    /// </summary>
    public RawContent Raw(string? html)
    {
        return new RawContent(html ?? string.Empty);
    }

    /// <summary>
    /// Renders the canonical URL link tag.
    /// </summary>
    public RawContent CanonicalUrl()
    {
        var model = GetBaseModel();
        if (model?.GeneratedUrl != null)
        {
            return new RawContent($@"<link rel=""canonical"" href=""{model.GeneratedUrl}"" />");
        }
        return new RawContent(string.Empty);
    }

    /// <summary>
    /// Renders a series navigation list.
    /// </summary>
    public RawContent RenderSeries(string className = "snow-series")
    {
        var series = GetSeriesFromModel();
        if (series == null || series.Parts.Count == 0)
        {
            return new RawContent(string.Empty);
        }

        var result = new StringBuilder();
        result.AppendFormat(@"<ul class=""{0}"">", className);

        foreach (var part in series.Parts)
        {
            if (!string.IsNullOrEmpty(part.Value.Url) && part.Key != series.Current)
            {
                result.AppendFormat(@"<li><a href=""{0}"">{1}</a></li>", part.Value.Url, part.Value.Name);
            }
            else
            {
                result.AppendFormat(@"<li>{0}</li>", part.Value.Name);
            }
        }

        result.Append("</ul>");
        return new RawContent(result.ToString());
    }

    /// <summary>
    /// Renders a series navigation list for a specific post.
    /// </summary>
    public RawContent RenderSeries(IPost? post, string className = "snow-series")
    {
        if (post?.Series == null || post.Series.Parts.Count == 0)
        {
            return new RawContent(string.Empty);
        }

        var series = post.Series;
        var result = new StringBuilder();
        result.AppendFormat(@"<ul class=""{0}"">", className);

        foreach (var part in series.Parts)
        {
            if (!string.IsNullOrEmpty(part.Value.Url) && part.Key != series.Current)
            {
                result.AppendFormat(@"<li><a href=""{0}"">{1}</a></li>", part.Value.Url, part.Value.Name);
            }
            else
            {
                result.AppendFormat(@"<li>{0}</li>", part.Value.Name);
            }
        }

        result.Append("</ul>");
        return new RawContent(result.ToString());
    }

    /// <summary>
    /// Renders Google Analytics tracking code.
    /// </summary>
    public RawContent RenderGoogleAnalytics(string trackingCode)
    {
        return new RawContent($@"<script type=""text/javascript"">
var _gaq = _gaq || [];

_gaq.push(['_setAccount', '{trackingCode}']);
_gaq.push(['_trackPageview']);
        
(function () {{
    var ga = document.createElement('script');
    ga.type = 'text/javascript';
    ga.async = true;
    ga.src = ('https:' == document.location.protocol ? 'https://ssl' : 'http://www') + '.google-analytics.com/ga.js';
    var s = document.getElementsByTagName('script')[0];
    s.parentNode.insertBefore(ga, s);
}})();
</script>");
    }

    /// <summary>
    /// Renders Google Universal Analytics tracking code.
    /// </summary>
    public RawContent RenderGoogleUniversalAnalytics(string trackingCode, string domain)
    {
        return new RawContent($@"<script type=""text/javascript"">
  (function(i,s,o,g,r,a,m){{i['GoogleAnalyticsObject']=r;i[r]=i[r]||function(){{
  (i[r].q=i[r].q||[]).push(arguments)}},i[r].l=1*new Date();a=s.createElement(o),
  m=s.getElementsByTagName(o)[0];a.async=1;a.src=g;m.parentNode.insertBefore(a,m)
  }})(window,document,'script','//www.google-analytics.com/analytics.js','ga');

  ga('create', '{trackingCode}', '{domain}');
  ga('send', 'pageview');

</script>");
    }

    /// <summary>
    /// Renders Disqus comments embed code.
    /// </summary>
    public RawContent RenderDisqusComments(string shortName)
    {
        var model = GetBaseModel();
        var url = model?.GeneratedUrl ?? string.Empty;

        if (string.IsNullOrEmpty(url))
        {
            return new RawContent(string.Empty);
        }

        return new RawContent($@"<div id=""disqus_thread""></div>
<script>
    var reset_disqus = function(){{
        DISQUS.reset({{
            reload: true,
            config: function () {{
                this.page.url = '{url}';
            }}
        }});
    }};

    var disqus_shortname = '{shortName}';
    var disqus_url = '{url}';

    (function() {{
        var dsq = document.createElement('script'); dsq.type = 'text/javascript'; dsq.async = true;
        dsq.src = '//' + disqus_shortname + '.disqus.com/embed.js';
        (document.getElementsByTagName('head')[0] || document.getElementsByTagName('body')[0]).appendChild(dsq);
    }})();

    window.addEventListener('orientationchange', reset_disqus);
</script>
<noscript>Please enable JavaScript to view the <a href=""http://disqus.com/?ref_noscript"">comments powered by Disqus.</a></noscript>
<a href=""http://disqus.com"" class=""dsq-brlink"">comments powered by <span class=""logo-disqus"">Disqus</span></a>");
    }

    /// <summary>
    /// Renders a Gravatar image.
    /// </summary>
    public RawContent RenderGravatarImage(string? email, int size = 0)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return new RawContent(string.Empty);
        }

        var hash = CreateMd5Hash(email.Trim().ToLowerInvariant());
        var url = size > 0 
            ? $"https://www.gravatar.com/avatar/{hash}?s={size}" 
            : $"https://www.gravatar.com/avatar/{hash}";
        
        return new RawContent($@"<img src=""{url}"" />");
    }

    private static string CreateMd5Hash(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
        var hashBytes = md5.ComputeHash(inputBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    private Snow.Engine.ViewModels.BasePostViewModel? GetBaseModel()
    {
        return _page.Model as Snow.Engine.ViewModels.BasePostViewModel;
    }

    private ISeries? GetSeriesFromModel()
    {
        if (_page.Model is Snow.Engine.ViewModels.BasePostViewModel vm)
        {
            // The model might be a PostViewModel with Series property
            var modelType = _page.Model.GetType();
            var seriesProperty = modelType.GetProperty("Series");
            if (seriesProperty != null)
            {
                return seriesProperty.GetValue(_page.Model) as ISeries;
            }
        }

        // Try dynamic access for v1 compatibility
        try
        {
            dynamic model = _page.Model!;
            return model.Series as ISeries;
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// Represents raw HTML content that should not be encoded.
/// </summary>
public sealed class RawContent
{
    public object Value { get; }

    public RawContent(object value)
    {
        Value = value;
    }

    public override string ToString() => Value?.ToString() ?? string.Empty;
}
