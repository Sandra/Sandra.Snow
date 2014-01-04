namespace Snow.StaticFileProcessors
{
    using System.IO;
    using Extensions;
    using Nancy.Testing;

    public class StaticFileProcessor : BaseProcessor
    {
        public override string ProcessorName
        {
            get { return ""; }
        }

        protected override void Impl(SnowyData snowyData, SnowSettings settings)
        {
            TestModule.GeneratedUrl = settings.SiteUrl + "/" + DestinationName.Trim(new[] { '/' }) + "/";

            var result = snowyData.Browser.Post("/static");

            result.ThrowIfNotSuccessful(SourceFile);

            if (!Directory.Exists(Destination))
            {
                Directory.CreateDirectory(Destination);
            }

            File.WriteAllText(Path.Combine(Destination, "index.html"), result.Body.AsString());
        }
    }
}