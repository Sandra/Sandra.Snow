namespace Snow.Tests.Extensions
{
    using System;
    using System.Collections.Generic;
    using FakeItEasy;
    using Models;
    using Nancy.ViewEngines.Razor;
    using Snow.Extensions;
    using ViewModels;
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
    var reset_disqus = function(){
        DISQUS.reset({
            reload: true,
            config: function () {
                //this.page.identifier = '';
                this.page.url = 'http://www.philliphaydon.com/test/url';
                //this.page.title = '';
            }
        });
    };

    var disqus_shortname = 'philliphaydon';
    var disqus_url = 'http://www.philliphaydon.com/test/url';

    (function() {
        var dsq = document.createElement('script'); dsq.type = 'text/javascript'; dsq.async = true;
        dsq.src = '//' + disqus_shortname + '.disqus.com/embed.js';
        (document.getElementsByTagName('head')[0] || document.getElementsByTagName('body')[0]).appendChild(dsq);
    })();

    window.addEventListener('orientationchange', reset_disqus);
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

        [Fact]
        public void Given_Post_With_No_Series_Should_Return_Empty_String()
        {
            var htmlHelper = A.Fake<HtmlHelpers<PostViewModel>>();
            const string expected = @"";

            htmlHelper.Model = new PostViewModel();

            Assert.Equal(expected, htmlHelper.RenderSeries().ToHtmlString());
        }

        [Fact]
        public void Given_First_Post_In_Series_Should_Generate_Unordered_List_With_No_Links()
        {
            var htmlHelper = A.Fake<HtmlHelpers<PostViewModel>>();
            const string expected = @"<ul class=""snow-series""><li>Part 1</li><li>Part 2</li><li>Part 3</li></ul>";

            htmlHelper.Model = new PostViewModel
            {
                Series = new Series
                {
                    Name = "123",
                    Current = 1,
                    Parts = new SortedList<int, Series.Part>
                    {
                        { 1, new Series.Part { Name = "Part 1", Url = "/2013/03/part-1" } },
                        { 2, new Series.Part { Name = "Part 2" } },
                        { 3, new Series.Part { Name = "Part 3" } }
                    }
                }
            };

            Assert.Equal(expected, htmlHelper.RenderSeries().ToHtmlString());
        }

        [Fact]
        public void Given_Second_Post_In_Series_Should_Have_Link_On_First_Part()
        {
            var htmlHelper = A.Fake<HtmlHelpers<PostViewModel>>();
            const string expected = @"<ul class=""snow-series""><li><a href=""/2013/03/part-1"">Part 1</a></li><li>Part 2</li><li>Part 3</li></ul>";

            htmlHelper.Model = new PostViewModel
            {
                Series = new Series
                {
                    Name = "123",
                    Current = 2,
                    Parts = new SortedList<int, Series.Part>
                    {
                        { 1, new Series.Part { Name = "Part 1", Url = "/2013/03/part-1" } },
                        { 2, new Series.Part { Name = "Part 2", Url = "/2013/03/part-2" } },
                        { 3, new Series.Part { Name = "Part 3" } }
                    }
                }
            };

            Assert.Equal(expected, htmlHelper.RenderSeries().ToHtmlString());
        }
    }
}