namespace Snow.StaticFileProcessors
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
            ParseDirectories(snowyData);

            var result = snowyData.Browser.Post("/static");

            result.ThrowIfNotSuccessful(SourceFile);

            if (!Directory.Exists(Destination))
            {
                Directory.CreateDirectory(Destination);
            }

            File.WriteAllText(Path.Combine(Destination, "index.html"), result.Body.AsString());
        }
    }
}