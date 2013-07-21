namespace Sandra.Snow.PreCompiler.StaticFileProcessors
{
    using System.IO;
    using System.Linq;
    using Nancy.Testing;

    public class CategoriesProcessor : BaseProcessor
    {
        public override string ProcessorName
        {
            get { return "categories"; }
        }

        public override ModeEnum Mode
        {
            get { return ModeEnum.Each; }
        }

        public override void Process(SnowyData snowyData)
        {
            foreach (var tempCategory in TestModule.Categories)
            {
                var category = tempCategory;

                var posts = snowyData.Files.Select(x => x.Post).Where(x => x.Categories.Contains(category.Name));

                TestModule.CategoriesInPost = posts.ToList();

                //TestModule.Data = fileData;
                var result = snowyData.Browser.Post("/static");

                result.StatusCode.ThrowIfNotSuccessful();

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