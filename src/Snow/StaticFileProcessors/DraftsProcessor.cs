namespace Snow.StaticFileProcessors
{
    using Extensions;
    using Nancy.Testing;
    using System.IO;

    public class DraftsProcessor : BaseProcessor
    {
        public override string ProcessorName
        {
            get { return "drafts"; }
        }

        protected override void Impl(SnowyData snowyData, SnowSettings settings)
        {
            foreach (var draft in TestModule.Drafts)
            {
                TestModule.GeneratedUrl = settings.SiteUrl + "/drafts" + draft.Url;

                var result = snowyData.Browser.Post("/static");

                result.ThrowIfNotSuccessful(SourceFile);

                var outputFolder = Path.Combine(snowyData.Settings.Output, "drafts", draft.Url);

                if (!Directory.Exists(outputFolder))
                {
                    Directory.CreateDirectory(outputFolder);
                }

                File.WriteAllText(Path.Combine(outputFolder, "index.html"), result.Body.AsString());
            }
        }
    }
}