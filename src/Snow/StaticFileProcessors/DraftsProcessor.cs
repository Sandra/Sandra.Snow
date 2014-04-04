namespace Snow.StaticFileProcessors
{
    using Enums;

    public class DraftsProcessor : StaticFileProcessor
    {
        public override string ProcessorName
        {
            get { return "drafts"; }
        }

        protected override void Impl(SnowyData snowyData, SnowSettings settings)
        {
            TestModule.Published = Published.Draft;
            
            base.Impl(snowyData, settings);
        }
    }
}