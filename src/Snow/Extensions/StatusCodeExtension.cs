namespace Snow.Extensions
{
    using Exceptions;
    using Nancy;
    using Nancy.Testing;

    public static class StatusCodeExtension
    {
        public static void ThrowIfNotSuccessful(this HttpStatusCode code)
        {
            if (code != HttpStatusCode.OK)
            {
                throw new FileProcessingException("Failed to generate some file...");
            }
        }

        public static void ThrowIfNotSuccessful(this BrowserResponse response, string fileName)
        {
            var body = response.Body.AsString();

            //if (result.StatusCode != HttpStatusCode.OK)
            //Crappy check because Nancy returns 200 on a compilation error :(
            if (body.Contains("<title>Razor Compilation Error</title>") &&
                body.Contains("<p>We tried, we really did, but we just can't compile your view.</p>"))
            {
                throw new FileProcessingException("Processing failed composing " + fileName);
            }
        }
    }
}