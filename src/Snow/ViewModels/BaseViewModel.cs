namespace Snow.ViewModels
{
    using System.Collections.Generic;
    using Enums;
    using Models;

    public abstract class BaseViewModel : DynamicDictionary
    {
        /// <summary>
        /// Title of current view. Will generally be in order from most to least
        /// specific, with final value always being site name.
        /// </summary>
        public IEnumerable<string> HeaderTitleChain { get; set; }
        /// <summary>
        /// All posts
        /// </summary>
        public List<Post> Posts { get; set; }
        public List<Page> Pages { get; set; }

        /// <summary>
        /// All draft posts
        /// </summary>
        public List<Post> Drafts { get; set; }

        public string GeneratedDate { get; set; }
        public List<MonthYear> MonthYearList { get; set; }
        public List<Category> AllCategories { get; set; }
        public string Keywords { get; set; }
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