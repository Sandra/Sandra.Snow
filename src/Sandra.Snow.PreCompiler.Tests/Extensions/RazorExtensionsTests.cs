namespace Sandra.Snow.PreCompiler.Tests.Extensions
{
    using System;
    using FakeItEasy;
    using Nancy.ViewEngines.Razor;
    using Sandra.Snow.PreCompiler.Extensions;
    using Sandra.Snow.PreCompiler.ViewModels;
    using Xunit;

    public class RazorExtensionsTests
    {
        [Fact]
        public void Given_Valid_Email_Should_Return_Img_Element_Formatted_Correctly()
        {
            const string testEmail = "phillip@example.com";
            const string expected =
                @"<img src=""http://www.gravatar.com/avatar/608955cd42dcd78ed8b548c2012d6ff2.jpg?"" />";

            var htmlHelper = A.Fake<HtmlHelpers<dynamic>>();

            var actual = htmlHelper.RenderGravatarImage(testEmail);

            Assert.Equal(expected, actual.ToHtmlString());
        }

        [Fact]
        public void Given_An_Empty_Email_Should_Return_Empty_String()
        {
            const string testEmail = "";
            const string expected = "";

            var htmlHelper = A.Fake<HtmlHelpers<dynamic>>();

            var actual = htmlHelper.RenderGravatarImage(testEmail);

            Assert.Equal(expected, actual.ToHtmlString());
        }

        [Fact]
        public void Given_Model_With_No_SiteUrl_Should_Throw()
        {
            var htmlHelper = A.Fake<HtmlHelpers<PostViewModel>>();

            htmlHelper.Model = new PostViewModel
            {
                Url = "/test/url",
                Settings = new SnowSettings()
            };

            Assert.Throws<ArgumentException>(() => htmlHelper.RenderDisqusComments("philliphaydon"));
        }

        [Fact]
        public void Given_Valid_Model_Should_Generate_Valid_EmbedCode()
        {
            var htmlHelper = A.Fake<HtmlHelpers<PostViewModel>>();
            const string expected = @"<div id=""disqus_thread""></div>
<script>
    var disqus_shortname = 'philliphaydon';
    var disqus_url = 'http://www.philliphaydon.com/test/url';

    (function() {
        var dsq = document.createElement('script'); dsq.type = 'text/javascript'; dsq.async = true;
        dsq.src = '//' + disqus_shortname + '.disqus.com/embed.js';
        (document.getElementsByTagName('head')[0] || document.getElementsByTagName('body')[0]).appendChild(dsq);
    })();
</script>
<noscript>Please enable JavaScript to view the <a href=""http://disqus.com/?ref_noscript"">comments powered by Disqus.</a></noscript>
<a href=""http://disqus.com"" class=""dsq-brlink"">comments powered by <span class=""logo-disqus"">Disqus</span></a>";

            htmlHelper.Model = new PostViewModel
            {
                Url = "/test/url",
                Settings = new SnowSettings
                {
                    SiteUrl = "http://www.philliphaydon.com"
                }
            };

            Assert.Equal(expected, htmlHelper.RenderDisqusComments("philliphaydon").ToHtmlString());
        }
    }
}