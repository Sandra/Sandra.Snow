namespace Snow.ViewModels
{
    using System.Collections.Generic;
    using Enums;
    using Models;

    public abstract class BaseViewModel : DynamicDictionary
    {
        /// <summary>
        /// All posts
        /// </summary>
        public List<Post> Posts { get; set; }

        /// <summary>
        /// All draft posts
        /// </summary>
        public List<Post> Drafts { get; set; }

        public string GeneratedDate { get; set; }
        public List<MonthYear> MonthYearList { get; set; }
        public List<Category> AllCategories { get; set; }
        public List<string> Keywords { get; set; }
        public Category Category { get; set; }
        public string GeneratedUrl { get; set; }
        public Published Published { get; set; }

        public class MonthYear
        {
            public string Url { get; set; }
            public string Title { get; set; }
            public int Count { get; set; }
        }
    }
}