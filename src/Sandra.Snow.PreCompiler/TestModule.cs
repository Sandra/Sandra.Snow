using System;
using System.Collections.Generic;
using Nancy;
using Nancy.ModelBinding;

namespace Sandra.Snow.PreCompiler
{
    internal class TestModule : NancyModule
    {
        //Data changes on iterations
        public static FileData Data { get; set; }
        public static StaticFile StaticFile { get; set; }

        //Properties change on iterations
        public static IList<Post> PostsPaged { get; set; }
        public static IList<Post> CategoriesInPost { get; set; }

        //Properties are set and never change...
        public static IList<Post> Posts { get; set; }
        public static IList<Category> Categories { get; set; }

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
                var model = new
                {
                    CategoriesInPost,
                    Categories,
                    Posts,
                    PostsPaged
                };

                return View[StaticFile.File, model];
            };
        }
    }
}