namespace Snow
{
    using System.Collections.Generic;
    using Nancy.ViewEngines.Razor;

    public class RazorConfig : IRazorConfiguration
    {
        public IEnumerable<string> GetAssemblyNames()
        {
            yield return "Snow";
        }

        public IEnumerable<string> GetDefaultNamespaces()
        {
            yield return "Snow.Extensions";
            yield return "Nancy.ViewEngines.Razor";
            yield return "System.Globalization";
            yield return "System.Collections.Generic";
            yield return "System.Linq";
        }

        public bool AutoIncludeModelNamespace { get { return true; } }
    }
}