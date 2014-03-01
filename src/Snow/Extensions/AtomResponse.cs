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

    public class AtomResponse : Response
    {
        private const string UrlRegex = @"(?<=<(a|img)\s+[^>]*?(href|src)=(?<q>['""]))(?!https?://)(?<url>/?.+?)(?=\k<q>)";
        private readonly string siteUrl;
        private readonly string feedfileName;
        private readonly string author;
        private readonly string authorEmail;
        private string AtomTitle { get; set; }

        public AtomResponse(IEnumerable<Post> model, string atomTitle, string siteUrl, string author, string authorEmail, string feedfileName)
        {
            this.siteUrl = siteUrl;
            this.feedfileName = feedfileName;
            this.author = author;
            this.authorEmail = authorEmail;

            AtomTitle = atomTitle;
            Contents = GetXmlContents(model);
            ContentType = "application/atom+xml";
            StatusCode = HttpStatusCode.OK;
        }

        private Action<Stream> GetXmlContents(IEnumerable<Post> model)
        {
            var items = new List<SyndicationItem>();

            foreach (var post in model)
            {
                // Replace all relative urls with full urls.
                var contentHtml = Regex.Replace(post.Content, UrlRegex, m => siteUrl.TrimEnd('/') + "/" + m.Value.TrimStart('/'));
                var excerptHtml = Regex.Replace(post.Content, UrlRegex, m => siteUrl.TrimEnd('/') + "/" + m.Value.TrimStart('/'));

                var item = new SyndicationItem(
                    post.Title,
                    contentHtml,
                    new Uri(siteUrl + post.Url)
                    )
                {
                    Id = siteUrl + post.Url,
                    LastUpdatedTime = post.Date.ToUniversalTime(),
                    PublishDate = post.Date.ToUniversalTime(),
                    Content = new TextSyndicationContent(contentHtml, TextSyndicationContentKind.Html),
                    Summary = new TextSyndicationContent(excerptHtml, TextSyndicationContentKind.Html),
                };

                items.Add(item);
            }

            var feed = new SyndicationFeed(
                AtomTitle,
                AtomTitle, /* Using Title also as Description */
                new Uri(siteUrl + "/" + feedfileName),
                items)
                {
                    Id = siteUrl + "/",
                    LastUpdatedTime = new DateTimeOffset(DateTime.Now),
                    Generator = "Sandra.Snow Atom Generator"
                };
            feed.Authors.Add(new SyndicationPerson(authorEmail, author, siteUrl));

            var link = new SyndicationLink(new Uri(siteUrl + "/" + feedfileName))
            {
                RelationshipType = "self",
                MediaType = "text/html",
                Title = AtomTitle
            };
            feed.Links.Add(link);


            var formatter = new Atom10FeedFormatter(feed);

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