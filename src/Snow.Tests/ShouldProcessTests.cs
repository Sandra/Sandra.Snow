namespace Snow.Tests
{
    using Enums;
    using Models;
    using Xunit;

    public class ShouldProcessTests
    {
        [Fact]
        public void Archive_should_only_include_published_posts()
        {
            Assert.True(ShouldProcess.Archive(new Post { Published = Published.True }));
            Assert.False(ShouldProcess.Archive(new Post { Published = Published.Draft }));
            Assert.False(ShouldProcess.Archive(new Post { Published = Published.Private }));
        }

        [Fact]
        public void Categories_should_only_include_published_posts()
        {
            Assert.True(ShouldProcess.Category(new Post { Published = Published.True }));
            Assert.False(ShouldProcess.Category(new Post { Published = Published.Private }));
            Assert.False(ShouldProcess.Category(new Post { Published = Published.Draft }));
        }
    }
}