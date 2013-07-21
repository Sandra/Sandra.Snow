using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandra.Snow.PreCompiler.StaticFileProcessors
{
    using Nancy.Testing;

    public abstract class BaseProcessor
    {
        public abstract string ProcessorName { get; }

        public bool Is(string processorName)
        {
            return processorName.ToLower(CultureInfo.InvariantCulture)
                                .Equals(ProcessorName.ToLower(CultureInfo.InvariantCulture));
        }

        public abstract void Process(SnowyData snowyData);
    }

    public class SnowyData
    {
        public SnowSettings Settings { get; set; }
        public Browser Browser { get; set; }
        public IList<FileData> Files { get; set; }
    }
}
