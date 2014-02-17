namespace Snow.Extensions
{
    using System.Collections.Generic;
    using Models;
    using Nancy;

    public static class FormatterExtensions
    {
        public static Response AsRSS(this IResponseFormatter formatter, IEnumerable<Post> model, string rssTitle, string siteUrl, string feedfileName)
        {
            return new RssResponse(model, rssTitle, siteUrl, feedfileName);
        }

        public static Response AsAtom(this IResponseFormatter formatter, IEnumerable<Post> model, string atomTitle, string siteUrl, string feedAuthor, string feedAuthorEmail, string feedfileName)
        {
            return new AtomResponse(model, atomTitle, siteUrl, feedAuthor, feedAuthorEmail, feedfileName);
        }

        public static Response AsSiteMap(this IResponseFormatter formatter, IEnumerable<Post> model, string siteUrl)
        {
            return new SitemapResponse(model, siteUrl);
        }
    }
}