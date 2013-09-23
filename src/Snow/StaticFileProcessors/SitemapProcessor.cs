﻿namespace Snow.StaticFileProcessors
{
    using System.IO;
    using Nancy.Testing;

    class SitemapProcessor : BaseProcessor
    {
        public override string ProcessorName
        {
            get { return "sitemap"; }
        }

        public override void Process(SnowyData snowyData, SnowSettings settings)
        {
            ParseDirectories(snowyData);

            var result = snowyData.Browser.Post("/sitemap");

            var outputFolder = snowyData.Settings.Output;

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            File.WriteAllText(Path.Combine(outputFolder, SourceFile), result.Body.AsString());
        }
    }
}
