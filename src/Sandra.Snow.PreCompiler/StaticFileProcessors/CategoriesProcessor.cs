namespace Sandra.Snow.PreCompiler.StaticFileProcessors
{
    using System;
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

        public override void Process(SnowyData snowyData, SnowSettings settings)
        {
            foreach (var tempCategory in TestModule.Categories)
            {
                var category = tempCategory;

                var posts = snowyData.Files.Where(x => x.Categories.Contains(category.Name));

                TestModule.Category = category;
                TestModule.CategoriesInPost = posts.ToList();
                
                var result = snowyData.Browser.Post("/static");

                result.ThrowIfNotSuccessful(snowyData.File.File);

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