namespace Sandra.Snow.PreCompiler.StaticFileProcessors
{
    using System.Collections.Generic;
    using Nancy.Testing;

    public class SnowyData
    {
        public SnowSettings Settings { get; set; }
        public Browser Browser { get; set; }
        public IList<FileData> Files { get; set; }
        public StaticFile File { get; set; }
    }
}