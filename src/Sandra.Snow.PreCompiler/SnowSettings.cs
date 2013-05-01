using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sandra.Snow.PreCompiler
{
    public class SnowSettings
    {
        public SnowSettings()
        {
            CurrentDir = string.Empty;
            CurrentSnowDir = string.Empty;
        }

        private string _posts;
        private string _layouts;
        private string _output;

        public string CurrentDir { get; private set; }
        public string CurrentSnowDir { get; set; }

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

        public string UrlFormat { get; set; }
        public string[] CopyDirectories { get; set; }
        public IEnumerable<StaticFile> ProcessStaticFiles { get; set; }

        public static SnowSettings Default(string directory)
        {
            return new SnowSettings
            {
                Posts = "_posts",
                Layouts = "_layouts",
                Output = "Website",
                UrlFormat = "",
                CopyDirectories = new string[] { },
                ProcessStaticFiles = Enumerable.Empty<StaticFile>(),
                CurrentDir = directory ?? "",
                CurrentSnowDir = Path.Combine(directory ?? "", "Snow")
            };
        }

    }

    public class StaticFile
    {
        public StaticFile()
        {
            //Mode = ModeEnum.Single;
        }

        public ModeEnum Mode { get; set; }
        public string File { get; set; }
        public string Property { get; set; }
    }

    public enum ModeEnum
    {
        Single,
        Each
    }
}