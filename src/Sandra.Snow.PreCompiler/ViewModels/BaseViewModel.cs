namespace Sandra.Snow.PreCompiler.ViewModels
{
    using System.Collections.Generic;
    using Models;

    public abstract class BaseViewModel
    {
        public string GeneratedDate { get; set; }
        public List<MonthYear> MonthYearList { get; set; }
        public List<Category> Categories { get; set; }
        public Category Category { get; set; }

        public class MonthYear
        {
            public string Url { get; set; }
            public string Title { get; set; }
            public int Count { get; set; }
        }
    }
}