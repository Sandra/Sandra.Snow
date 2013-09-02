namespace Snow
{
    using System.Collections.Generic;
    using Nancy.ViewEngines.Razor;

    public class RazorConfig : IRazorConfiguration
    {
        public IEnumerable<string> GetAssemblyNames()
        {
            yield return "Sandra.Snow.PreCompiler";
        }

        public IEnumerable<string> GetDefaultNamespaces()
        {
            yield return "Sandra.Snow.PreCompiler.Extensions";
            yield return "System.Globalization";
            yield return "System.Collections.Generic";
            yield return "System.Linq";
        }

        public bool AutoIncludeModelNamespace { get { return true; } }
    }
}