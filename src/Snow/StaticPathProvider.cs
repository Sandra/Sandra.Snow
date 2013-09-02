namespace Sandra.Snow.PreCompiler
{
    using Nancy;

    public class StaticPathProvider : IRootPathProvider
    {
        public static string Path { get; set; }

        public string GetRootPath()
        {
            return Path;
        }
    }
}