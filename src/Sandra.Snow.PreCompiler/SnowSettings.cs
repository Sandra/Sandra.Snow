using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sandra.Snow.PreCompiler
{
    public class SnowSettings
    {
        private string _posts;
        private string _layouts;
        private string _output;

        public SnowSettings()
        {
            CurrentDir = string.Empty;
            CurrentSnowDir = string.Empty;
        }

        public string CurrentDir { get; private set; }
        public string CurrentSnowDir { get; set; }
        public string UrlFormat { get; set; }
        public string[] CopyDirectories { get; set; }

        public string Posts
        {
            get { return _posts; }
            set { _posts = Path.Combine(CurrentSnowDir, value); }
        }

        public string Layouts
        {
            get { return _layouts; }
            set { _layouts = Path.Combine(CurrentSnowDir, value); }
        }

        public string Output
        {
            get { return _output; }
            set { _output = Path.Combine(CurrentDir, value); }
        }
        
        public IList<StaticFile> ProcessStaticFiles { get; set; }
        
        public static SnowSettings Default(string directory)
        {
            return new SnowSettings
            {
                Posts = "_posts",
                Layouts = "_layouts",
                Output = "Website",
                UrlFormat = "",
                CopyDirectories = new string[] { },
                ProcessStaticFiles = new List<StaticFile>(),
                CurrentDir = directory ?? "",
                CurrentSnowDir = Path.Combine(directory ?? "", "Snow")
            };
        }
    }
}