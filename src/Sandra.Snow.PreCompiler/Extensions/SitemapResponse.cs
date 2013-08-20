namespace Sandra.Snow.PreCompiler.Extensions
{
    using System.IO;
    using System.ServiceModel.Syndication;
    using System.Xml;
    using System.Xml.Linq;
    using Models;
    using Nancy;
    using System;
    using System.Collections.Generic;

    public class SitemapResponse : Response
    {
        private readonly string siteUrl;

        public SitemapResponse(IEnumerable<Post> model, string siteUrl)
        {
            this.siteUrl = siteUrl;
            Contents = GetXmlContents(model);
            ContentType = "application/xml";
            StatusCode = HttpStatusCode.OK;
        }

        private Action<Stream> GetXmlContents(IEnumerable<Post> model)
        {
            XNamespace blank = XNamespace.Get(@"http://www.sitemaps.org/schemas/sitemap/0.9");

            var xDocument = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement(blank+"urlset", new XAttribute("xmlns", blank.NamespaceName)));

            foreach (Post post in model)
            {
                var xElement = new XElement(blank+"url",
                                                    new XElement(blank + "loc", this.siteUrl + post.Url),
                                                    new XElement(blank + "lastmod", post.Date.ToString("s")),
                                                    new XElement(blank + "changefreq", "weekly"),
                                                    new XElement(blank + "priority", "1.00"));

                xDocument.Root.Add(xElement);
            }

            return stream =>
            {
                using (XmlWriter writer = XmlWriter.Create(stream))
                {

                    xDocument.Save(writer);
                }
            };
        }
    }

}
