namespace Sandra.Snow.PreCompiler.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    public class ContentViewModel : BaseViewModel
    {
        public IList<Post> CategoriesInPost { get; set; }
        public IList<Category> Categories { get; set; }
        public IList<Post> Posts { get; set; }
        public IList<Post> PostsPaged { get; set; }
        public IList<MonthYear> MonthYearList { get; set; }
        public Dictionary<int, Dictionary<int, List<Post>>> PostsGroupedByYearThenMonth { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
        public int NextPage { get; set; }
        public int PreviousPage { get; set; }

        public string GetMonth(int month)
        {
            return DateTime.ParseExact("2013/" + month + "/1", "yyyy/M/d", CultureInfo.InvariantCulture).ToString("MMMM");
        }
    }
}