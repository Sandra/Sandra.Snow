namespace Snow.StaticFileProcessors
{
    using Models;
    using Nancy.Testing;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class RssProcessor : BaseProcessor
    {
        public override string ProcessorName
        {
            get { return "rss"; }
        }

        protected override void Impl(SnowyData snowyData, SnowSettings settings)
        {
            var postsForRss = GetPostsForRss(snowyData.Files);

            TestModule.PostsPaged = postsForRss;

            var result = snowyData.Browser.Post("/rss");

            var outputFolder = snowyData.Settings.Output;

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            File.WriteAllText(Path.Combine(outputFolder, SourceFile), result.Body.AsString());
        }

        internal List<Post> GetPostsForRss(IList<Post> files)
        {
            return files.Where(ShouldProcess.Feed).ToList();
        }
    }
}
