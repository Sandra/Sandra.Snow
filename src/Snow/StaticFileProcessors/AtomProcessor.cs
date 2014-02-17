namespace Snow.StaticFileProcessors
{
    using Enums;
    using Models;
    using Nancy.Testing;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class AtomProcessor : BaseProcessor
    {
        public override string ProcessorName
        {
            get { return "atom"; }
        }

        protected override void Impl(SnowyData snowyData, SnowSettings settings)
        {
            var postsForAtom = GetPostsForAtom(snowyData.Files);

            TestModule.PostsPaged = postsForAtom;

            var result = snowyData.Browser.Post("/atom");

            var outputFolder = snowyData.Settings.Output;

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            File.WriteAllText(Path.Combine(outputFolder, SourceFile), result.Body.AsString());
        }

        internal List<Post> GetPostsForAtom(IList<Post> files)
        {
            return files.Where(ShouldProcess).Take(10).ToList();
        }

        internal bool ShouldProcess(Post post)
        {
            return post.Published == Published.True;
        }
    }
}
