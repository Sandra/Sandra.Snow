using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandra.Snow.PreCompiler.StaticFileProcessors
{
    public abstract class BaseProcessor
    {
        public abstract string ProcessorName { get; }

        public bool Is(string processorName)
        {
            return processorName.Equals(ProcessorName);
        }

        public abstract void Process(SnowyData snowyData);
    }

    public class SnowyData
    {
        public SnowSettings Settings { get; set; }
    }
}
