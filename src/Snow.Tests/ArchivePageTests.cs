namespace Snow.Tests
{
    using Enums;
    using Models;
    using Xunit;

    public class ArchivePageTests
    {
        [Fact]
        public void Should_process()
        {
            Assert.True(ArchivePage.ShouldProcess(new Post { Published = Published.True }));
            Assert.False(ArchivePage.ShouldProcess(new Post { Published = Published.Private }));
            Assert.False(ArchivePage.ShouldProcess(new Post { Published = Published.Draft }));
        }
    }
}