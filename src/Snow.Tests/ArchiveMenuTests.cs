namespace Snow.Tests
{
    using Enums;
    using Models;
    using Xunit;

    public class ArchiveMenuTests
    {
        [Fact]
        public void Should_only_include_published_posts()
        {
            Assert.True(ArchiveMenu.ShouldProcess(new Post { Published = Published.True }));
            Assert.False(ArchiveMenu.ShouldProcess(new Post { Published = Published.Draft }));
            Assert.False(ArchiveMenu.ShouldProcess(new Post { Published = Published.Private }));
        }
    }
}