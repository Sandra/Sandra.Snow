namespace Sandra.Snow.PreCompiler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Models;
    using Nancy;
    using ViewModels;

    internal class TestModule : NancyModule
    {
        private static readonly string Date = DateTime.Now.ToString("O");
        public static string GeneratedDate
        {
            get { return Date; }
        }

        //Data changes on iterations
        public static Post Data { get; set; }
        public static StaticFile StaticFile { get; set; }

        //Properties change on iterations
        public static IList<Post> PostsPaged { get; set; }
        public static IList<Post> CategoriesInPost { get; set; }
        public static int PageNumber { get; set; }

        //Properties are set and never change...
        public static IList<Post> Posts { get; set; }
        public static IList<Category> Categories { get; set; }
        public static Dictionary<int, Dictionary<int, List<Post>>> PostsGroupedByYearThenMonth { get; set; }
        public static int TotalPages { get; set; }

        public static bool HasPreviousPage { get; set; }
        public static bool HasNextPage { get; set; }

        public static SnowSettings Settings { get; set; }

        public static IList<BaseViewModel.MonthYear> MonthYear { get; set; }

        public TestModule()
        {
            // Generates the post from Markdown
            Get["/post/{file}"] = x => View[(string)x.file];

            // Generates any static page given all site content
            Post["/static"] = x =>
            {
                var siteContent = new ContentViewModel
                {
                    CategoriesInPost = CategoriesInPost,
                    Categories = Categories.Select(y => new BaseViewModel.Category
                    {
                        Name = y.Name,
                        Url = y.Url,
                        Count = y.Count
                    }).OrderBy(cat => cat.Name).ToList(),
                    Posts = Posts,
                    PostsPaged = PostsPaged,
                    PostsGroupedByYearThenMonth = PostsGroupedByYearThenMonth,
                    HasPreviousPage = HasPreviousPage,
                    HasNextPage = HasNextPage,
                    NextPage = PageNumber + 1,
                    PreviousPage = PageNumber - 1,
                    MonthYearList = MonthYear,
                    GeneratedDate = GeneratedDate,
                };

                return View[StaticFile.File, siteContent];
            };

            // Generates an actual post based on the Markdown content
            // with a SiteContent property for access to everything
            Post["/compose"] = x =>
            {
                var categories = Data.Categories.Select(category => new BaseViewModel.Category
                    {
                        Url = category.ToLower().Replace(" ", "-"),
                        Name = category
                    }).OrderBy(cat => cat.Name).ToList();

                var result = new PostViewModel
                {
                    PostContent = Data.Content,
                    PostDate = Data.Date,
                    Layout = Data.Layout,
                    Title = Data.Title,
                    GeneratedDate = GeneratedDate,
                    Url = Data.Url,
                    Categories = categories,
                    MonthYearList = MonthYear,
                    Author = Data.Author,
                    Email = Data.Email,
                    Settings = Settings,
                    Series = Data.Series,
                    MetaDescription = Data.MetaDescription
                };

                return View[result.Layout, result];
            };
        }
    }
}