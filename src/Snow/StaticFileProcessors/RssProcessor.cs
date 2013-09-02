namespace Snow.StaticFileProcessors
{
    using System.IO;
    using System.Linq;
    using Nancy.Testing;

    public class RssProcessor : BaseProcessor
    {
        public override string ProcessorName
        {
            get { return "rss"; }
        }

        public override void Process(SnowyData snowyData, SnowSettings settings)
        {
            var postsForRss = snowyData.Files.Take(10).ToList();
            TestModule.PostsPaged = postsForRss;
            TestModule.StaticFile = snowyData.File;

            var result = snowyData.Browser.Post("/rss");
            
            var outputFolder = snowyData.Settings.Output;

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }
            
            File.WriteAllText(Path.Combine(outputFolder, snowyData.File.File), result.Body.AsString());
        }
    }
}
