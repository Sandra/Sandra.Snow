﻿namespace Snow
{
    using System.Collections.Generic;
    using System.IO;

    public class SnowSettings
    {
        private string _layouts;
        private string _output;
        private string _posts;

        public SnowSettings()
        {
            CurrentDir = string.Empty;
        }

        /// <summary>
        /// Default author
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Default author email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Default layout file to use for posts
        /// </summary>
        public string DefaultPostLayout { get; set; }

        public string CurrentDir { get; private set; }
        public string UrlFormat { get; set; }
        public string[] CopyDirectories { get; set; }

        public string PostsRaw { get; private set; }
        public string Posts
        {
            get { return _posts; }
            set
            {
                PostsRaw = value;
                _posts = Path.Combine(CurrentDir, value);
            }
        }

        public string LayoutsRaw { get; private set; }
        public string Layouts
        {
            get { return _layouts; }
            set
            {
                LayoutsRaw = value;
                _layouts = Path.Combine(CurrentDir, value);
            }
        }

        public string ThemesDir = "themes/";
        public string Theme { get; set; }
        
        public string Output
        {
            get { return _output; }
            set { _output = Path.Combine(CurrentDir, value); }
        }

        public List<StaticFile> ProcessFiles { get; set; }
        
        public string SiteUrl { get; set; }

        public string BlogTitle { get; set; }

        public int PageSize { get; set; }

        public static SnowSettings Default(string directory)
        {
            return new SnowSettings
            {
                CurrentDir = directory.TrimEnd('/'),
                Posts = "_posts",
                Layouts = "_layouts",
                Theme = "default",
                Output = "Website",
                UrlFormat = "yyyy/MM/dd/slug",
                CopyDirectories = new string[] {},
                ProcessFiles = new List<StaticFile>(),
                PageSize = 10,
                Author = string.Empty,
                Email = string.Empty,
                DefaultPostLayout = "post"
            };
        }
    }
}