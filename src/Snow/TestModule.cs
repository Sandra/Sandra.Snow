namespace Snow
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Enums;
    using Extensions;
    using Models;
    using Nancy;
    using Nancy.Cookies;
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
        public static string StaticFile { get; set; }
        public static string GeneratedUrl { get; set; }
        public static Published Published { get; set; }

        //Properties change on iterations
        public static List<Post> PostsPaged { get; set; }
        public static List<Post> PostsInCategory { get; set; }
        public static Category Category { get; set; }
        public static int PageNumber { get; set; }

        //Properties are set and never change...
        public static List<Post> Posts { get; set; }
        public static List<Category> Categories { get; set; }
        public static Dictionary<int, Dictionary<int, List<Post>>> PostsGroupedByYearThenMonth { get; set; }
        public static int TotalPages { get; set; }

        public static bool HasPreviousPage { get; set; }
        public static bool HasNextPage { get; set; }

        public static SnowSettings Settings { get; set; }

        public static List<BaseViewModel.MonthYear> MonthYear { get; set; }

        public static List<Post> Drafts { get; set; }
        public static List<Page> Pages { get; set; }

        public TestModule()
        {
            // Generates any static page given all site content
            Post["/static"] = x =>
            {
                var siteContent = new ContentViewModel
                {
                    GeneratedUrl = GeneratedUrl,
                    PostsInCategory = PostsInCategory,
                    AllCategories = Categories,
                    Posts = Posts,
                    PostsPaged = PostsPaged,
                    Pages = Pages,
                    PostsGroupedByYearThenMonth = PostsGroupedByYearThenMonth,
                    HasPreviousPage = HasPreviousPage,
                    HasNextPage = HasNextPage,
                    NextPage = PageNumber + 1,
                    PreviousPage = PageNumber - 1,
                    MonthYearList = MonthYear,
                    GeneratedDate = GeneratedDate,
                    Category = Category,
                    Drafts = Drafts,
                    Published = Published
                };

                return View[StaticFile, siteContent];
            };

            // Generates an actual post based on the Markdown content
            // with a SiteContent property for access to everything
            Post["/compose"] = x =>
            {
                dynamic result = new PostViewModel
                {
                    Drafts = Drafts,
                    Posts = Posts,
                    Pages = Pages,
                    GeneratedUrl = GeneratedUrl,
                    PostContent = Data.Content,
                    PostDate = Data.Date,
                    Layout = Data.Layout,
                    Title = Data.Title,
                    GeneratedDate = GeneratedDate,
                    Url = Data.Url,
                    AllCategories = Categories,
                    Categories = Data.Categories.Select(c => new Category { Name = c }).ToList(),
                    Keywords = Data.Keywords,
                    MonthYearList = MonthYear,
                    Author = Data.Author,
                    Email = Data.Email,
                    Settings = Settings,
                    Series = Data.Series,
                    MetaDescription = Data.MetaDescription,
                    Published = Data.Published
                };

                result.Banana = "WOW!";

                return View[result.Layout, result];
            };

            Post["/rss"] = x => Response.AsRSS(Posts, Settings.BlogTitle, Settings.SiteUrl, StaticFile);

            Post["/atom"] = x => Response.AsAtom(Posts, Settings.BlogTitle, Settings.SiteUrl, Settings.Author, Settings.Email, StaticFile);

            Post["/sitemap"] = x =>
            {
                var publishedPosts = Posts.Where(post => post.Published == Published.True);
                
                return Response.AsSiteMap(publishedPosts, Settings.SiteUrl);
            };
        }
    }
}