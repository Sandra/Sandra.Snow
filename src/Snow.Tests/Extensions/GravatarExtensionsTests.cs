namespace Snow.Tests.Extensions
{
    using Snow.Extensions;
    using Xunit;

    public class GravatarExtensionsTests
    {
        [Fact]
        public void Given_No_Email_Should_Return_Empty_String()
        {
            const string testEmail = "";
            const string expected = "";

            var actual = testEmail.EmailToGravatar();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Given_Valid_Email_Should_Return_Correct_Avatar_Url_With_Hash()
        {
            const string testEmail = "phillip@example.com";
            const string expected = "http://www.gravatar.com/avatar/608955cd42dcd78ed8b548c2012d6ff2.jpg?";

            var actual = testEmail.EmailToGravatar();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Given_Valid_Email_Should_Trim_White_Space_And_Return_Correct_Avatar_Url_With_Hash()
        {
            const string testEmail = " phillip@example.com ";
            const string expected = "http://www.gravatar.com/avatar/608955cd42dcd78ed8b548c2012d6ff2.jpg?";

            var actual = testEmail.EmailToGravatar();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Given_Valid_Email_With_Size_Should_Return_Correct_Avatar_Url_With_Size_Param()
        {
            const string testEmail = "phillip@example.com";
            const string expected = "http://www.gravatar.com/avatar/608955cd42dcd78ed8b548c2012d6ff2.jpg?s=200";

            var actual = testEmail.EmailToGravatar(size: 200);

            Assert.Equal(expected, actual);
        }
    }
}