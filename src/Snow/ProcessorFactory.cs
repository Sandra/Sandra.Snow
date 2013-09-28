namespace Snow
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;
    using StaticFileProcessors;

    public class ProcessorFactory
    {
        private static readonly IList<BaseProcessor> Processors = new List<BaseProcessor>();

        static ProcessorFactory()
        {
            Processors = AppDomain.CurrentDomain
                                  .GetAssemblies()
                                  .SelectMany(x => x.GetTypes())
                                  .Where(x => x.IsSubclassOf(typeof (BaseProcessor)))
                                  .Select(x =>
                                  {
                                      x.Name.OutputIfDebug("Adding Processor: ");
                                      return (BaseProcessor) Activator.CreateInstance(x);
                                  })
                                  .ToList();
        }

        public static BaseProcessor Get(string name)
        {
            return Processors.SingleOrDefault(x => x.Is(name));
        }
    }
}