using System;
using System.Collections.Generic;
using Nancy;
using Nancy.ModelBinding;

namespace Sandra.Snow.PreCompiler
{
    using System.Globalization;

    internal class TestModule : NancyModule
    {
        //Data changes on iterations
        public static FileData Data { get; set; }
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

        public TestModule()
        {
            Get["/post/{file}"] = x => View[(string)x.file];

            Post["/compose"] = x =>
            {
                var result = new ComposableContent
                {
                    PostContent = Data.Content,
                    Layout = Data.Layout,
                    Title = Data.Title
                };

                return View[result.Layout, result];
            };

            Post["/static"] = x =>
            {
                var model = new PostData
                {
                    CategoriesInPost = CategoriesInPost,
                    Categories = Categories,
                    Posts = Posts,
                    PostsPaged = PostsPaged,
                    PostsGroupedByYearThenMonth = PostsGroupedByYearThenMonth,
                    HasPreviousPage = HasPreviousPage,
                    HasNextPage = HasNextPage,
                    NextPage = PageNumber + 1,
                    PreviousPage = PageNumber - 1
                };

                return View[StaticFile.File, model];
            };
        }
    }

    public class PostData
    {
        public IList<Post> CategoriesInPost { get; set; }
        public IList<Category> Categories { get; set; }
        public IList<Post> Posts { get; set; }
        public IList<Post> PostsPaged { get; set; }
        public Dictionary<int, Dictionary<int, List<Post>>> PostsGroupedByYearThenMonth { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
        public int NextPage { get; set; }
        public int PreviousPage { get; set; }

        public string GetMonth(int month)
        {
            return DateTime.ParseExact("2013/" + month + "/1", "yyyy/MM/dd", CultureInfo.InvariantCulture).ToString("MMMM");
        }
    }
}