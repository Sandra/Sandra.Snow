namespace Snow.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.ServiceModel.Syndication;
    using System.Xml;
    using Models;
    using Nancy;

    public class RssResponse : Response
    {
        private readonly string siteUrl;
        private readonly string feedfileName;
        private string RssTitle { get; set; }


        public RssResponse(IEnumerable<Post> model, string rssTitle, string siteUrl, string feedfileName)
        {
            this.siteUrl = siteUrl;
            this.feedfileName = feedfileName;
            this.RssTitle = rssTitle;


            this.Contents = GetXmlContents(model);
            this.ContentType = "application/rss+xml";
            this.StatusCode = HttpStatusCode.OK;
        }

        private Action<Stream> GetXmlContents(IEnumerable<Post> model)
        {
            var items = new List<SyndicationItem>();

            foreach (Post post in model)
            {

                var item = new SyndicationItem(
                    title: post.Title,
                    content: post.Content,
                    itemAlternateLink: new Uri(siteUrl + post.Url),
                    id: siteUrl + post.Url,
                    lastUpdatedTime: post.Date.ToUniversalTime()
                    );
                item.PublishDate = post.Date.ToUniversalTime();
                item.Summary = new TextSyndicationContent(post.ContentExcerpt, TextSyndicationContentKind.Plaintext);
                items.Add(item);
            }

            var feed = new SyndicationFeed(
                this.RssTitle,
                this.RssTitle, /* Using Title also as Description */
                new Uri(siteUrl + "/" + feedfileName),
                items);

            var formatter = new Rss20FeedFormatter(feed);

            return stream =>
            {
                using (XmlWriter writer = XmlWriter.Create(stream))
                {
                    formatter.WriteTo(writer);

                }
            };
        }
    }
}
