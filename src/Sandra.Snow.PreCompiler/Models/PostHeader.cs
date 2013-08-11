namespace Sandra.Snow.PreCompiler.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class PostHeader
    {
        public PostHeader()
        {
            Categories = Enumerable.Empty<string>();
        }

        public void SetSnowSettings(SnowSettings defaults)
        {
            Author = defaults.Author;
            Email = defaults.Email;
        }

        public void SetHeaderSettings(Dictionary<string, object> settings)
        {
            foreach (var setting in settings)
            {
                switch (setting.Key.ToLower())
                {
                    case "categories":
                    case "category":
                    {
                        var categories = ((string)setting.Value).Split(
                            new[] { "," },
                            StringSplitOptions.RemoveEmptyEntries);

                        Categories = categories.Select(x => x.Trim());
                        break;
                    }
                    case "title":
                    {
                        Title = (string)setting.Value;
                        break;
                    }
                    case "layout":
                    {
                        Layout = (string)setting.Value;
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
                    case "series":
                    {
                        Series = (Series)setting.Value;
                        break;
                    }
                }
            }
        }

        public Series Series { get; set; }

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
                    Url = Url,
                    Author = Author,
                    Email = Email
                };
            }
        }

        public string Author { get; set; }
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
        public string Url { get; set; }

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