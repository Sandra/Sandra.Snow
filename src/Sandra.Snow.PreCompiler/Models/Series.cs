namespace Sandra.Snow.PreCompiler.Models
{
    using System.Collections.Generic;

    public class Series
    {
        public Series()
        {
            Parts = new SortedList<int, string>();
        }

        public string Id { get; set; }
        public int Current { get; set; }
        public SortedList<int, string> Parts { get; set; }
    }
}