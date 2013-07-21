namespace Sandra.Snow.PreCompiler.Extensions
{
    using Nancy;
    using Sandra.Snow.PreCompiler.Exceptions;

    public static class StatusCodeExtension
    {
        public static void ThrowIfNotSuccessful(this HttpStatusCode code)
        {
            if (code != HttpStatusCode.OK)
            {
                throw new FileProcessingException("Failed to generate some file...");
            }
        }
    }
}