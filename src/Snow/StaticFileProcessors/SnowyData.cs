namespace Sandra.Snow.PreCompiler.StaticFileProcessors
{
    using System.Collections.Generic;
    using Models;
    using Nancy.Testing;

    public class SnowyData
    {
        public SnowSettings Settings { get; set; }
        public Browser Browser { get; set; }
        public IList<Post> Files { get; set; }
        public StaticFile File { get; set; }
    }
}