namespace Snow.StaticFileProcessors
{
    using System.IO;
    using Nancy.Testing;

    class SitemapProcessor : BaseProcessor
    {
        public override string ProcessorName
        {
            get { return "sitemap"; }
        }

        protected override void Impl(SnowyData snowyData, SnowSettings settings)
        {
            var result = snowyData.Browser.Post("/sitemap");

            var outputFolder = snowyData.Settings.PostsOutput;

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            File.WriteAllText(Path.Combine(outputFolder, SourceFile), result.Body.AsString());
        }
    }
}
