namespace Sandra.Snow.PreCompiler.Models
{
    using System;
    using System.Collections.Generic;

    public class Post
    {
        public string Title { get; set; }
        public string Content { get; set; }

        public string ContentExcerpt
        {
            get { return Content.Split(new[] {"<!--excerpt-->"}, StringSplitOptions.None)[0]; }
        }

        public string Url { get; set; }
        public IEnumerable<string> Categories { get; set; }
        public DateTime Date { get; set; }

        public string DateFormatted
        {
            get { return Date.ToString("dd MMM yyyy"); }
        }
    }
}