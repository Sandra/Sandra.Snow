namespace Snow
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using MarkdownDeep;
    using Nancy;
    using Nancy.Responses;
    using Nancy.ViewEngines;
    
    public class CustomMarkDownViewEngine : IViewEngine
    {
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
            
            var html = renderContext.ViewCache.GetOrAdd(viewLocationResult, result => ConvertMarkdown(viewLocationResult));

            response.Contents = stream =>
            {
                var writer = new StreamWriter(stream);
                writer.Write(html);
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

            return new Markdown{ExtraMode = true, SafeMode = false}.Transform(content);
        }
    }
}