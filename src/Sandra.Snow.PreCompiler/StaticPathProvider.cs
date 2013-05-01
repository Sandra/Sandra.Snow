using Nancy;

namespace Sandra.Snow.PreCompiler
{
    public class StaticPathProvider : IRootPathProvider
    {
        public static string Path { get; set; }

        public string GetRootPath()
        {
            return Path;
        }
    }
}