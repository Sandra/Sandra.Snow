﻿namespace Sandra.Snow.PreCompiler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Sandra.Snow.PreCompiler.Models;

    public class PostHeaderSettings
    {
        public PostHeaderSettings(Dictionary<string, object> settings, SnowSettings defaults)
        {
            Author = defaults.Author;
            Email = defaults.Email;

            Categories = Enumerable.Empty<string>();

            foreach (var setting in settings)
            {
                switch (setting.Key.ToLower())
                {
                    case "categories":
                    case "category":
                    {
                        var categories = ((string) setting.Value).Split(
                            new[] {","},
                            StringSplitOptions.RemoveEmptyEntries);

                        Categories = categories.Select(x => x.Trim());
                        break;
                    }
                    case "title":
                    {
                        Title = (string) setting.Value;
                        break;
                    }
                    case "layout":
                    {
                        Layout = (string) setting.Value;
                        break;
                    }
                    case "author":
                    {
                        Author = (string)setting.Value;
                        break;
                    }
                    case "email":
                    {
                        Email = (string)setting.Value;
                        break;
                    }
                }
            }
        }

        public Post Post
        {
            get
            {
                return new Post
                {
                    Title = Title,
                    Categories = Categories,
                    Content = Content,
                    Date = Date,
                    Url = Slug,
                    Author = Author,
                    Email = Email
                };
            }
        }

        /// <summary>
        /// Default author
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Default author email
        /// </summary>
        public string Email { get; set; }

        public IEnumerable<string> Categories { get; set; }

        public string Title { get; set; }
        public string Content { get; set; }
        public string Layout { get; set; }
        public IDictionary<string, dynamic> Settings { get; set; }
        public string RawSettings { get; set; }
        public string FileName { get; set; }

        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public string Slug { get; set; }

        public DateTime Date { get; set; }
    }

    public class BaseData
    {
        public IEnumerable<Post> Posts { get; set; }
        public IEnumerable<string> Categories { get; set; }
    }

    public class CategoryData : BaseData
    {
        public CategoryData(BaseData baseData)
        {
            Posts = baseData.Posts;
            Categories = baseData.Categories;
        }

        public IEnumerable<Post> PostsInCategory { get; set; }
    }
}