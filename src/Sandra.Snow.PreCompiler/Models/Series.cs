namespace Sandra.Snow.PreCompiler.Models
{
    using System.Collections.Generic;

    public class Series
    {
        public Series()
        {
            Parts = new SortedList<int, Part>();
        }

        public string Name { get; set; }
        public int Current { get; set; }
        public SortedList<int, Part> Parts { get; set; }

        public class Part
        {
            public string Name { get; set; }
            public string Url { get; set; }

            public bool HasUrl()
            {
                return !string.IsNullOrWhiteSpace(Url);
            }
        }
    }
}