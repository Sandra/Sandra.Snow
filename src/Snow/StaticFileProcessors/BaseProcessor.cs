namespace Snow.StaticFileProcessors
{
    using System;
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

        protected void ParseDirectories(SnowyData snowyData)
        {
            var source = snowyData.File.File;

            var sourceFile = source;
            var destinationDirectory = source.Substring(0, snowyData.File.File.IndexOf('.'));

            if (source.Contains(" => "))
            {
                var directorySplit = source.Split(new[] { " => " }, StringSplitOptions.RemoveEmptyEntries);

                sourceFile = directorySplit[0];
                destinationDirectory = directorySplit[1];
            }

            SourceFile = sourceFile;
            Destination = destinationDirectory;
        }

        protected string SourceFile { get; set; }
        protected string Destination { get; set; }
    }
}