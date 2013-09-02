namespace Sandra.Snow.PreCompiler.StaticFileProcessors
{
    using System.IO;
    using Extensions;
    using Nancy.Testing;

    public class StaticFileProcessor : BaseProcessor
    {
        public override string ProcessorName
        {
            get { return ""; }
        }

        public override void Process(SnowyData snowyData, SnowSettings settings)
        {
            var result = snowyData.Browser.Post("/static");

            result.ThrowIfNotSuccessful(snowyData.File.File);

            var outputFolder = Path.Combine(snowyData.Settings.Output, snowyData.File.File.Substring(0, snowyData.File.File.IndexOf('.')));

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            File.WriteAllText(Path.Combine(outputFolder, "index.html"), result.Body.AsString());
        }
    }
}