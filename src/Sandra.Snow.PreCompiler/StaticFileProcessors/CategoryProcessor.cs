namespace Sandra.Snow.PreCompiler.StaticFileProcessors
{
    using System.IO;
    using Extensions;
    using Nancy.Testing;

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

            result.ThrowIfNotSuccessful(snowyData.File.File);

            var outputFolder = Path.Combine(snowyData.Settings.Output, "category");

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            File.WriteAllText(Path.Combine(outputFolder, "index.html"), result.Body.AsString());
        }
    }
}