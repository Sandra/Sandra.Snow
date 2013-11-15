namespace Snow.Extensions
{
    using System.Text.RegularExpressions;
    using Exceptions;
    using Nancy.Testing;

    public static class StatusCodeExtension
    {
        public static void ThrowIfNotSuccessful(this BrowserResponse response, string fileName)
        {
            var body = 
                response.Body.AsString();

            if (body.Contains("<title>Razor Compilation Error</title>") &&
                body.Contains("<p>We tried, we really did, but we just can't compile your view.</p>"))
            {
                var match =
                    Regex.Match(body, "<pre id=\"errorContents\">(?<content>.*)&lt;!DOCTYPE html&gt;", RegexOptions.IgnoreCase | RegexOptions.Singleline);

                var message = (match.Success) ?
                    match.Groups["content"].Value :
                    string.Empty;

                throw new FileProcessingException(message);
            }
        }
    }
}