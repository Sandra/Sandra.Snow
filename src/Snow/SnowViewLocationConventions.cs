namespace Snow
{
    using System;
    using System.Collections.Generic;
    using Nancy.Conventions;
    using Nancy.ViewEngines;

    public class SnowViewLocationConventions : IConvention
    {
        public static SnowSettings Settings { get; set; }

        public void Initialise(NancyConventions conventions)
        {
            ConfigureViewLocationConventions(conventions);
        }

        public Tuple<bool, string> Validate(NancyConventions conventions)
        {
            return Tuple.Create(true, string.Empty);
        }

        private static void ConfigureViewLocationConventions(NancyConventions conventions)
        {
            conventions.ViewLocationConventions = new List<Func<string, object, ViewLocationContext, string>>
            {
                (viewName, model, viewLocationContext) => Settings.PostsRaw.TrimEnd('/') + "/" + viewName,
                (viewName, model, viewLocationContext) => Settings.LayoutsRaw.TrimEnd('/') + "/" + viewName,
                (viewName, model, viewLocationContext) => viewName
            };
        }
    }
}