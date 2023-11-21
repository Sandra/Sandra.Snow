namespace Snow.StaticFileProcessors
{
    using Enums;
    using Snow.Models;

    public class DraftsProcessor : StaticFileProcessor
    {
        public override string ProcessorName
        {
            get { return "drafts"; }
        }

        protected override void Impl(SnowyData snowyData, SnowSettings settings)
        {
            TestModule.Published = Published.Draft;
            TestModule.HeaderTitleChain = new[] { "Drafts", TestModule.Settings.BlogTitle };
            base.Impl(snowyData, settings);
        }
    }
}