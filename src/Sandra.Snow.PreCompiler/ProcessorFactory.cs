﻿namespace Sandra.Snow.PreCompiler
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition.Hosting;
    using System.ComponentModel.Composition.Primitives;
    using System.ComponentModel.Composition.ReflectionModel;
    using System.IO;
    using System.Linq;
    using StaticFileProcessors;

    public class ProcessorFactory
    {
        private static readonly IList<BaseProcessor> Processors = new List<BaseProcessor>();

        static ProcessorFactory()
        {
            Processors = MefHelpers.GetExportedTypes<BaseProcessor>()
                                   .Select(x => (BaseProcessor) Activator.CreateInstance(x))
                                   .ToList();
        }

        public static BaseProcessor Get(string name)
        {
            return Processors.SingleOrDefault(x => x.Is(name));
        }
    }

    public static class MefHelpers
    {
        public static IList<Type> GetExportedTypes<T>()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var catalog = new AggregateCatalog();

            // Handle Self-host or unit tests.
            catalog.Catalogs.Add(new DirectoryCatalog(path, "*"));

            // Handle WebSites. (ie. the bin directory is a child directory off the root-website directory).
            var pathWithBin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");
            if (Directory.Exists(pathWithBin))
            {
                catalog.Catalogs.Add(new DirectoryCatalog(pathWithBin, "*"));
            }

            return catalog.Parts
                          .Select(part => ComposablePartExportType<T>(part))
                          .Where(t => t != null)
                          .ToList();
        }

        private static Type ComposablePartExportType<T>(ComposablePartDefinition part)
        {
            return part.ExportDefinitions.Any(
                def => def.Metadata.ContainsKey("ExportTypeIdentity") &&
                       def.Metadata["ExportTypeIdentity"].Equals(typeof (T).FullName))
                ? ReflectionModelServices.GetPartType(part).Value
                : null;
        }
    }
}