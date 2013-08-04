namespace Sandra.Snow.PreCompiler.StaticFileProcessors
{
    using System.ComponentModel.Composition;
    using System.Globalization;

    [InheritedExport]
    public abstract class BaseProcessor
    {
        public abstract string ProcessorName { get; }
        public abstract bool IterateModel { get; }

        public bool Is(string processorName, bool iterateModel)
        {
            return
                processorName.ToLower(CultureInfo.InvariantCulture)
                             .Equals(ProcessorName.ToLower(CultureInfo.InvariantCulture)) &&
                iterateModel == IterateModel;
        }

        public abstract void Process(SnowyData snowyData);
    }
}