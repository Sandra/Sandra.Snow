namespace Snow
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using MarkdownSharp;
    using Nancy;
    using Nancy.Responses;
    using Nancy.ViewEngines;
    using Nancy.ViewEngines.Markdown;
    using Nancy.ViewEngines.SuperSimpleViewEngine;

    public class CustomMarkDownViewEngine : IViewEngine
    {
        private readonly SuperSimpleViewEngine engineWrapper;

        public CustomMarkDownViewEngine(SuperSimpleViewEngine engineWrapper)
        {
            this.engineWrapper = engineWrapper;
        }

        public IEnumerable<string> Extensions
        {
            get { return new[] {"md", "markdown"}; }
        }

        public void Initialize(ViewEngineStartupContext viewEngineStartupContext)
        {
        }

        public Response RenderView(ViewLocationResult viewLocationResult, dynamic model, IRenderContext renderContext)
        {
            var response = new HtmlResponse();

            var html = renderContext.ViewCache.GetOrAdd(
                viewLocationResult, result => { return ConvertMarkdown(viewLocationResult); });

            var engineHost = new MarkdownViewEngineHost(
                new NancyViewEngineHost(renderContext), renderContext,
                Extensions);
            var renderHtml = engineWrapper.Render(html, model, engineHost);

            response.Contents = stream =>
            {
                var writer = new StreamWriter(stream);
                writer.Write(renderHtml);
                writer.Flush();
            };

            return response;
        }

        public string ConvertMarkdown(ViewLocationResult viewLocationResult)
        {
            var content = viewLocationResult.Contents().ReadToEnd();

            // Parse out the post settings 
            var startOfSettingsIndex = content.IndexOf("---", StringComparison.InvariantCultureIgnoreCase);

            if (startOfSettingsIndex >= 0)
            {
                var endOfSettingsIndex = content.IndexOf(
                    "---", startOfSettingsIndex + 3, StringComparison.InvariantCultureIgnoreCase);

                endOfSettingsIndex += 3;
                content = content.Substring(endOfSettingsIndex, content.Length - endOfSettingsIndex);
            }

            return new Markdown().Transform(content);
        }
    }

    //public class MarkdownViewEngineHost : IViewEngineHost
    //{
    //    private readonly IViewEngineHost viewEngineHost;
    //    private readonly IRenderContext renderContext;
    //    private readonly IEnumerable<string> validExtensions;
    //    private readonly Markdown parser;

    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="MarkdownViewEngineHost"/> class.
    //    /// </summary>
    //    /// <param name="viewEngineHost">A decorator <see cref="IViewEngineHost"/></param>
    //    /// <param name="renderContext">The render context.</param>
    //    /// <param name="viewExtensions">The allowed extensions</param>
    //    public MarkdownViewEngineHost(IViewEngineHost viewEngineHost, IRenderContext renderContext, IEnumerable<string> viewExtensions)
    //    {
    //        this.viewEngineHost = viewEngineHost;
    //        this.renderContext = renderContext;
    //        this.validExtensions = viewExtensions;
    //        this.Context = this.renderContext.Context;
    //        this.parser = new Markdown();
    //    }

    //    /// <summary>
    //    /// Context object of the host application.
    //    /// </summary>
    //    /// <value>An instance of the context object from the host.</value>
    //    public object Context { get; private set; }

    //    /// <summary>
    //    /// Html "safe" encode a string
    //    /// </summary>
    //    /// <param name="input">Input string</param>
    //    /// <returns>Encoded string</returns>
    //    public string HtmlEncode(string input)
    //    {
    //        return this.viewEngineHost.HtmlEncode(input);
    //    }

    //    /// <summary>
    //    /// Get the contenst of a template
    //    /// </summary>
    //    /// <param name="templateName">Name/location of the template</param>
    //    /// <param name="model">Model to use to locate the template via conventions</param>
    //    /// <returns>Contents of the template, or null if not found</returns>
    //    public string GetTemplate(string templateName, object model)
    //    {
    //        var viewLocationResult = this.renderContext.LocateView(templateName, model);

    //        if (viewLocationResult == null)
    //        {
    //            return "[ERR!]";
    //        }

    //        var templateContent = viewLocationResult.Contents.Invoke().ReadToEnd();

    //        if (viewLocationResult.Name.ToLower() == "master" && validExtensions.Any(x => x.Equals(viewLocationResult.Extension, StringComparison.OrdinalIgnoreCase)))
    //        {
    //            return MarkdownViewengineRender.RenderMasterPage(templateContent);
    //        }

    //        if (!validExtensions.Any(x => x.Equals(viewLocationResult.Extension, StringComparison.OrdinalIgnoreCase)))
    //        {
    //            return viewLocationResult.Contents.Invoke().ReadToEnd();
    //        }

    //        return parser.Transform(templateContent);
    //    }

    //    /// <summary>
    //    /// Gets a uri string for a named route
    //    /// </summary>
    //    /// <param name="name">Named route name</param>
    //    /// <param name="parameters">Parameters to use to expand the uri string</param>
    //    /// <returns>Expanded uri string, or null if not found</returns>
    //    public string GetUriString(string name, params string[] parameters)
    //    {
    //        return this.viewEngineHost.GetUriString(name, parameters);
    //    }

    //    /// <summary>
    //    /// Expands a path to include any base paths
    //    /// </summary>
    //    /// <param name="path">Path to expand</param>
    //    /// <returns>Expanded path</returns>
    //    public string ExpandPath(string path)
    //    {
    //        return this.viewEngineHost.ExpandPath(path);
    //    }

    //    /// <summary>
    //    /// Get the anti forgery token form element
    //    /// </summary>
    //    /// <returns>String containin the form element</returns>
    //    public string AntiForgeryToken()
    //    {
    //        return this.viewEngineHost.AntiForgeryToken();
    //    }
    //}

    //public static class MarkdownViewengineRender
    //{
    //    /// <summary>
    //    /// A regex for removing paragraph tags that the parser inserts on unknown content such as @Section['Content']
    //    /// </summary>
    //    /// <remarks>
    //    ///  <p>		- matches the literal string "<p>"
    //    ///  (		- creates a capture group, so that we can get the text back by backreferencing in our replacement string
    //    ///  @		- matches the literal string "@"
    //    ///  [^<]*	- matches any character other than the "<" character and does this any amount of times
    //    ///  )		- ends the capture group
    //    ///  </p>	- matches the literal string "</p>"
    //    /// </remarks>
    //    private static readonly Regex ParagraphSubstitution = new Regex("<p>(@[^<]*)</p>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    //    /// <summary>
    //    /// Renders stand alone / master page
    //    /// </summary>
    //    /// <param name="templateContent">Template content</param>
    //    /// <returns>HTML converted to markdown</returns>
    //    public static string RenderMasterPage(string templateContent)
    //    {
    //        var second =
    //           templateContent.Substring(
    //               templateContent.IndexOf("<!DOCTYPE html>", StringComparison.OrdinalIgnoreCase),
    //               templateContent.IndexOf("<body", StringComparison.OrdinalIgnoreCase));

    //        var third = templateContent.Substring(second.Length);

    //        var forth = templateContent.Substring(second.Length, third.IndexOf(">", StringComparison.Ordinal) + 1);

    //        var header = second + forth;

    //        var toConvert = templateContent.Substring(header.Length,
    //                                                  (templateContent.IndexOf("</body>", StringComparison.Ordinal) -
    //                                                   (templateContent.IndexOf(forth, StringComparison.Ordinal) + forth.Length)));

    //        var footer =
    //            templateContent.Substring(templateContent.IndexOf("</body>", StringComparison.OrdinalIgnoreCase));

    //        var parser = new Markdown();

    //        var html = parser.Transform(toConvert.Trim());

    //        var serverHtml = ParagraphSubstitution.Replace(html, "$1");

    //        //TODO: The "Replace" is simply for unit testing HTML/MD strings. Probably needs improving
    //        return string.Concat(header, serverHtml, footer).Replace("\r\n", "").Replace("\n", "").Replace("\r", "");
    //    }
    //}
}