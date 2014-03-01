namespace Snow.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.ServiceModel.Syndication;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;
    using Models;
    using Nancy;
    using Nancy.IO;

    public class RssResponse : Response
    {
        private const string UrlRegex = @"(?<=<(a|img)\s+[^>]*?(href|src)=(?<q>['""]))(?!https?://)(?<url>/?.+?)(?=\k<q>)";
        private readonly string siteUrl;
        private readonly string feedfileName;
        private string RssTitle { get; set; }

        public RssResponse(IEnumerable<Post> model, string rssTitle, string siteUrl, string feedfileName)
        {
            this.siteUrl = siteUrl;
            this.feedfileName = feedfileName;

            RssTitle = rssTitle;
            Contents = GetXmlContents(model);
            ContentType = "application/rss+xml";
            StatusCode = HttpStatusCode.OK;
        }

        private Action<Stream> GetXmlContents(IEnumerable<Post> model)
        {
            var items = new List<SyndicationItem>();

            foreach (var post in model)
            {
                // Replace all relative urls with full urls.
                var newHtml = Regex.Replace(post.Content, UrlRegex, m => siteUrl.TrimEnd('/') + "/" + m.Value.TrimStart('/'));
                var summaryHtml = Regex.Replace(post.ContentExcerpt, UrlRegex, m => siteUrl.TrimEnd('/') + "/" + m.Value.TrimStart('/'));

                var item = new SyndicationItem(
                    post.Title,
                    newHtml,
                    new Uri(siteUrl + post.Url)
                    )
                {
                    Id = siteUrl + post.Url,
                    LastUpdatedTime = post.Date.ToUniversalTime(),
                    PublishDate = post.Date.ToUniversalTime(),
                    Summary = new TextSyndicationContent(summaryHtml, TextSyndicationContentKind.Html)
                };

                items.Add(item);
            }

            var feed = new SyndicationFeed(
                RssTitle,
                RssTitle, /* Using Title also as Description */
                new Uri(siteUrl + "/" + feedfileName),
                items);

            var formatter = new Rss20FeedFormatter(feed);

            return stream =>
            {
                var encoding = new UTF8Encoding(false);
                var streamWrapper = new UnclosableStreamWrapper(stream);

                using (var writer = new XmlTextWriter(streamWrapper, encoding))
                {
                    formatter.WriteTo(writer);
                }
            };
        }
    }
}