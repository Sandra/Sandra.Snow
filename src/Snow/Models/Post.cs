namespace Snow.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Post
    {
        public Post()
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
                    case "metadescription":
                        {
                            MetaDescription = (string)setting.Value;
                            break;
                        }
                }
            }
        }

        public string MetaDescription { get; set; }

        public Series Series { get; set; }

        public string ContentExcerpt
        {
            get { return Content.Split(new[] { "<!--excerpt-->" }, StringSplitOptions.None)[0]; }
        }

        public string Author { get; set; }
        public string Email { get; set; }

        public IEnumerable<string> Categories { get; set; }

        public string Title { get; set; }
        public string Content { get; set; }
        public string Layout { get; set; }
        public IDictionary<string, dynamic> Settings { get; set; }
        public string FileName { get; set; }

        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public string Url { get; set; }

        public DateTime Date { get; set; }

        /// <summary>
        /// Raw unparsed header from markdown file
        /// </summary>
        public string MarkdownHeader { get; set; }
    }
}