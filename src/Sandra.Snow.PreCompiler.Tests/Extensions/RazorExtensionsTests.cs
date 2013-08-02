namespace Sandra.Snow.PreCompiler.Tests.Extensions
{
    using FakeItEasy;
    using Nancy.ViewEngines.Razor;
    using Sandra.Snow.PreCompiler.Extensions;
    using Xunit;

    public class RazorExtensionsTests
    {
        [Fact]
        public void Given_Valid_Email_Should_Return_Img_Element_Formatted_Correctly()
        {
            const string testEmail = "phillip@example.com";
            const string expected = @"<img src=""http://www.gravatar.com/avatar/608955cd42dcd78ed8b548c2012d6ff2.jpg?"" />";

            var htmlHelper = A.Fake<HtmlHelpers<dynamic>>();

            var actual = htmlHelper.RenderGravatarImage(testEmail);

            Assert.Equal(expected, actual.ToHtmlString());
        }
    }
}