namespace Sandra.Snow.PreCompiler.StaticFileProcessors
{
    using System.IO;
    using Nancy.Testing;
    using Sandra.Snow.PreCompiler.Extensions;

    public class CategoryProcessor : BaseProcessor
    {
        public override string ProcessorName
        {
            get { return "categories"; }
        }

        public override bool IterateModel
        {
            get { return false; }
        }

        public override void Process(SnowyData snowyData)
        {
            var result = snowyData.Browser.Post("/static");

            result.StatusCode.ThrowIfNotSuccessful();

            var outputFolder = Path.Combine(snowyData.Settings.Output, "category");

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            File.WriteAllText(Path.Combine(outputFolder, "index.html"), result.Body.AsString());
        }
    }
}