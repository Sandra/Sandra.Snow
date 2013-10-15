namespace Snow.Tests.Processors
{
    using Enums;
    using Models;
    using StaticFileProcessors;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class CategoriesProcessorTests
    {
        [Fact]
        public void Should_get_posts()
        {
            var processor = new CategoriesProcessor();
            var category = new Category { Name = "MyCategory" };
            var posts = new List<Post>
                {
                    new Post{Published = Published.True, Categories = new[]{"MyCategory"}},
                    new Post{Published = Published.Draft, Categories = new[]{"MyCategory"}},
                    new Post{Published = Published.Private, Categories = new[]{"MyCategory"}},
                    new Post{Categories = new[]{"MyCategory1"}},
                };

            var postsFromCategory = processor.GetPosts(posts, category);

            Assert.Equal(1, postsFromCategory.Count());
            Assert.True(postsFromCategory.All(x => x.Published == Published.True));
        }

        [Fact]
        public void Should_process()
        {
            var processor = new CategoriesProcessor();

            Assert.True(processor.ShouldProcess(new Post { Published = Published.True }));
            Assert.False(processor.ShouldProcess(new Post { Published = Published.Private }));
            Assert.False(processor.ShouldProcess(new Post { Published = Published.Draft }));
        }
    }
}