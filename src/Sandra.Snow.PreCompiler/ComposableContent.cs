namespace Sandra.Snow.PreCompiler
{
    using System.Collections.Generic;

    internal class ComposableContent
    {
        public string Layout { get; set; }
        public string Title { get; set; }
        public string PostContent { get; set; }
        public IList<MonthYear> MonthYear { get; set; }
    }
}