namespace Sandra.Snow.PreCompiler.StaticFileProcessors
{
    using System.IO;
    using Nancy.Testing;
    using Sandra.Snow.PreCompiler.Extensions;

    public class StaticFileProcessor : BaseProcessor
    {
        public override string ProcessorName
        {
            get { return ""; }
        }

        public override ModeEnum Mode
        {
            get{ return ModeEnum.Single; }
        }

        public override void Process(SnowyData snowyData)
        {
            var result = snowyData.Browser.Post("/static");

            result.StatusCode.ThrowIfNotSuccessful();

            var outputFolder = Path.Combine(snowyData.Settings.Output, snowyData.File.File.Substring(0, snowyData.File.File.IndexOf('.')));

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            File.WriteAllText(Path.Combine(outputFolder, "index.html"), result.Body.AsString());
        }
    }
}