namespace Snow.StaticFileProcessors
{
    using System;
    using System.Globalization;
    using System.IO;
    using Enums;

    public abstract class BaseProcessor
    {
        public abstract string ProcessorName { get; }

        public bool Is(string processorName)
        {
            return processorName.ToLower(CultureInfo.InvariantCulture)
                                .Equals(ProcessorName.ToLower(CultureInfo.InvariantCulture));
        }

        public void Process(SnowyData snowyData, SnowSettings settings)
        {
            ParseDirectories(snowyData);
            TestModule.StaticFile = SourceFile;
            TestModule.Published = Published.True;

            Impl(snowyData, settings);
        }

        protected abstract void Impl(SnowyData snowyData, SnowSettings settings);

        protected void ParseDirectories(SnowyData snowyData)
        {
            var source = snowyData.File.File;

            var sourceFile = source;
            var destinationDirectory = Path.Combine(snowyData.Settings.PostsOutput, source.Substring(0, snowyData.File.File.IndexOf('.')));
            var destinationName = source.Substring(0, snowyData.File.File.IndexOf('.'));

            if (source.Contains(" => "))
            {
                var directorySplit = source.Split(new[] { " => " }, StringSplitOptions.RemoveEmptyEntries);

                sourceFile = directorySplit[0];
                destinationDirectory = Path.Combine(snowyData.Settings.PostsOutput, directorySplit[1]);
                destinationName = directorySplit[1];
            }

            SourceFile = sourceFile;
            Destination = destinationDirectory;
            DestinationName = destinationName;
        }

        protected string SourceFile { get; set; }
        protected string Destination { get; set; }
        protected string DestinationName { get; set; }
    }
}