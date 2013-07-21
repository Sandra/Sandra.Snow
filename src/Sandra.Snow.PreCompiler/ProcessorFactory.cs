using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandra.Snow.PreCompiler
{
    using Sandra.Snow.PreCompiler.StaticFileProcessors;

    public class ProcessorFactory
    {
        private static readonly IDictionary<string, BaseProcessor> Processors = new Dictionary<string, BaseProcessor>();

        static ProcessorFactory()
        {
            Processors.Add();
        }

        public static BaseProcessor Get(string name)
        {
            if (Processors.ContainsKey(name))
            {
                return Processors[name];
            }

            return null;
        }
    }
}