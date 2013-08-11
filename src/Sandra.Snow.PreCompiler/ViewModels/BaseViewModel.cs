namespace Sandra.Snow.PreCompiler.ViewModels
{
    using System.Collections.Generic;

    public abstract class BaseViewModel
    {
        public string GeneratedDate { get; set; }
        public IList<MonthYear> MonthYearList { get; set; }
        public IList<Category> Categories { get; set; }

        public class Category
        {
            public string Name { get; set; }
            public string Url { get; set; }
            public int Count { get; set; }
        }

        public class MonthYear
        {
            public string Url { get; set; }
            public string Title { get; set; }
            public int Count { get; set; }
        }
    }
}