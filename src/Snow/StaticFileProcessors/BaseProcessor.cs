namespace Sandra.Snow.PreCompiler.StaticFileProcessors
{
    using System.ComponentModel.Composition;
    using System.Globalization;

    [InheritedExport]
    public abstract class BaseProcessor
    {
        public abstract string ProcessorName { get; }

        public bool Is(string processorName)
        {
            return processorName.ToLower(CultureInfo.InvariantCulture)
                                .Equals(ProcessorName.ToLower(CultureInfo.InvariantCulture));
        }

        public abstract void Process(SnowyData snowyData, SnowSettings settings);
    }
}