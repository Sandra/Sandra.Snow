namespace Snow
{
    using System.Collections.Generic;
    using System.IO;

    public class SnowSettings
    {
        private string layouts;
        private string postsOutput;
        private string pagesOutput;
        private string posts;
        private string pages;

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
        public string DefaultPageLayout { get; set; }

        public string CurrentDir { get; private set; }
        public string PostUrlFormat { get; set; }
        public string PageUrlFormat { get; set; }
        public string[] CopyDirectories { get; set; }
        public string[] CopyFiles { get; set; }

        public string PostsRaw { get; private set; }
        public string PagesRaw { get; private set; }
         public string Posts
         {
           get { return posts; }
           set
           {
             PostsRaw = value;
             posts = Path.Combine(CurrentDir, value);
           }
         }
     
         public string Pages
         {
           get { return pages; }
           set
           {
             PagesRaw = value;
             pages = Path.Combine(CurrentDir, value);
           }
         }
     
         public string LayoutsRaw { get; private set; }
        public string Layouts
        {
            get { return layouts; }
            set
            {
                LayoutsRaw = value;
                layouts = Path.Combine(CurrentDir, value);
            }
        }

        public string ThemesDir = "themes/";
        public string Theme { get; set; }
        
        public string PostsOutput
        {
            get { return postsOutput; }
            set { postsOutput = Path.Combine(CurrentDir, value); }
        }

        public string PagesOutput
        {
          get { return pagesOutput; }
          set { pagesOutput = Path.Combine(CurrentDir, value); }
        }
    
        public List<StaticFile> ProcessFiles { get; set; }
        
        public string SiteUrl { get; set; }

        public string BlogTitle { get; set; }

        public int PageSize { get; set; }

        public int FeedSize { get; set; }

        public static SnowSettings Default(string directory)
        {
              return new SnowSettings
              {
                  CurrentDir = directory.TrimEnd('/'),
                  Posts = "_posts",
                  Layouts = "_layouts",
                  Theme = "default",
                  PostsOutput = "Website",
                  PagesOutput = "Website",
                  PostUrlFormat = "yyyy/MM/dd/slug",
                  PageUrlFormat = "slug",
                  CopyDirectories = new string[] {},
                  CopyFiles = new string[] {},
                  ProcessFiles = new List<StaticFile>(),
                  PageSize = 10,
                  FeedSize = 10,
                  Author = string.Empty,
                  Email = string.Empty,
                  DefaultPostLayout = "post",
                  DefaultPageLayout = "page"
              };
          }
    }
}