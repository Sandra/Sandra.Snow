namespace Sandra.Snow.PreCompiler.ViewModels
{
    public abstract class BaseViewModel
    {
        public string GeneratedDate { get; set; }

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