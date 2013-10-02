namespace Snow.StaticFileProcessors
{
    using System.IO;
    using System.Linq;
    using Extensions;
    using Nancy.Testing;

    public class CategoriesProcessor : BaseProcessor
    {
        public override string ProcessorName
        {
            get { return "categories"; }
        }

        protected override void Impl(SnowyData snowyData, SnowSettings settings)
        {
            foreach (var tempCategory in TestModule.Categories)
            {
                var category = tempCategory;

                var posts = snowyData.Files.Where(x => x.Categories.Contains(category.Name));

                TestModule.Category = category;
                TestModule.GeneratedUrl = settings.SiteUrl + "/category/" + category.Url + "/";
                TestModule.PostsInCategory = posts.ToList();
                
                var result = snowyData.Browser.Post("/static");

                result.ThrowIfNotSuccessful(SourceFile);

                var outputFolder = Path.Combine(snowyData.Settings.Output, "category", category.Url);

                if (!Directory.Exists(outputFolder))
                {
                    Directory.CreateDirectory(outputFolder);
                }

                File.WriteAllText(Path.Combine(outputFolder, "index.html"), result.Body.AsString());
            }
        }
    }
}