namespace Sandra.Snow.PreCompiler.StaticFileProcessors
{
    using System.IO;
    using Nancy.Testing;

    public class CategoryProcessor : BaseProcessor
    {
        public override string ProcessorName
        {
            get { return "categories"; }
        }

        public override ModeEnum Mode
        {
            get{ return ModeEnum.Single; }
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