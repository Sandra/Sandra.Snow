namespace Snow.Tests.Processors
{
    using Enums;
    using Models;
    using StaticFileProcessors;
    using System.Collections.Generic;
    using Xunit;

    public class RssProcessorTests
    {
        public class GetPostsForRssTests
        {
            [Fact]
            public void Should_take_ten()
            {
                var processor = new RssProcessor();

                var files = new List<Post>
                {
                   new Post{ Published = Published.True},
                   new Post{ Published = Published.True},
                   new Post{ Published = Published.True},
                   new Post{ Published = Published.True},
                   new Post{ Published = Published.True},
                   new Post{ Published = Published.True},
                   new Post{ Published = Published.True},
                   new Post{ Published = Published.True},
                   new Post{ Published = Published.True},
                   new Post{ Published = Published.True},
                   new Post{ Published = Published.True},
                   new Post{ Published = Published.True},
                   new Post{ Published = Published.True},
                   new Post{ Published = Published.True},
                };
                var posts = processor.GetPostsForRss(files);
                Assert.Equal(10, posts.Count);
            }

            [Fact]
            public void Should_only_take_published()
            {
                var processor = new RssProcessor();

                var files = new List<Post>
                {
                    new Post{ Published = Published.True},
                    new Post{ Published = Published.True},
                    new Post{ Published = Published.Private},
                    new Post{ Published = Published.Draft},
                };
                var posts = processor.GetPostsForRss(files);
                Assert.Equal(2, posts.Count);
            }
        }
    }
}