namespace Sandra.Snow.PreCompiler.StaticFileProcessors
{
    using System.ComponentModel.Composition;
    using System.Globalization;

    [InheritedExport]
    public abstract class BaseProcessor
    {
        public abstract string ProcessorName { get; }
        public abstract ModeEnum Mode { get; }

        public bool Is(string processorName, ModeEnum mode)
        {
            if (Mode != mode)
                return false;

            return processorName.ToLower(CultureInfo.InvariantCulture)
                                .Equals(ProcessorName.ToLower(CultureInfo.InvariantCulture));
        }

        public abstract void Process(SnowyData snowyData);
    }
}