namespace Snow.Tests.SettingParsers
{
    using Enums;
    using Models;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class HeaderParserTests
    {
        [Fact]
        public void Should_parse_parse_author()
        {
            var settings = new Dictionary<string, object>()
                {
                    {"author", "author name"}
                };

            var post = new Post();

            post.SetHeaderSettings(settings);
            Assert.Equal("author name", post.Author);
        }

        [Fact]
        public void Should_parse_parse_categories()
        {
            var settings = new Dictionary<string, object>()
                {
                    {"categories", "1, 2"}
                };

            var post = new Post();

            post.SetHeaderSettings(settings);
            Assert.Equal("1", post.Categories.First());
            Assert.Equal("2", post.Categories.Last());
        }

        [Fact]
        public void Should_parse_category()
        {
            var settings = new Dictionary<string, object>()
                {
                    {"category", "1, 2"}
                };

            var post = new Post();

            post.SetHeaderSettings(settings);
            Assert.Equal("1", post.Categories.First());
            Assert.Equal("2", post.Categories.Last());
        }

        [Fact]
        public void Should_parse_email()
        {
            var settings = new Dictionary<string, object>()
                {
                    {"email", "author@email.com"}
                };

            var post = new Post();

            post.SetHeaderSettings(settings);
            Assert.Equal("author@email.com", post.Email);
        }

        [Fact]
        public void Should_parse_layout()
        {
            var settings = new Dictionary<string, object>()
                {
                    {"layout", "layout"}
                };

            var post = new Post();

            post.SetHeaderSettings(settings);
            Assert.Equal("layout", post.Layout);
        }

        [Fact]
        public void Should_parse_meta_description()
        {
            var settings = new Dictionary<string, object>()
                {
                    {"metadescription", "meta description"}
                };

            var post = new Post();

            post.SetHeaderSettings(settings);
            Assert.Equal("meta description", post.MetaDescription);
        }

        [Fact]
        public void Should_parse_series()
        {
            var settings = new Dictionary<string, object>()
                {
                    {"series", new Series
                        {
                            Current = 2,
                            Name = "Series name",
                            Parts = new SortedList<int, Series.Part>
                                {
                                    {1, new Series.Part
                                        {
                                            Name = "Part1",
                                            Url = "/url/to/part1"
                                        }},
                                    {2, new Series.Part
                                        {
                                            Name = "Part2",
                                            Url = "/url/to/part2"
                                        }},
                                }
                        }}
                };

            var post = new Post();


            post.SetHeaderSettings(settings);

            var series = post.Series;

            Assert.Equal(2, series.Parts.Count);
        }

        [Fact]
        public void Should_parse_title()
        {
            var settings = new Dictionary<string, object>()
                {
                    {"title", "Post title"}
                };

            var post = new Post();

            post.SetHeaderSettings(settings);
            Assert.Equal("Post title", post.Title);
        }

        public class PublishedTests
        {
            [Fact]
            public void Published_should_default_to_true_when_missing()
            {
                var settings = new Dictionary<string, object>();

                var post = new Post();

                post.SetHeaderSettings(settings);
                Assert.Equal(Published.True, post.Published);
            }

            [Fact]
            public void Published_should_default_to_true_when_unknown_value()
            {
                var settings = new Dictionary<string, object>()
                 {
                     {"published", ""}
                 };

                var post = new Post();

                post.SetHeaderSettings(settings);
                Assert.Equal(Published.True, post.Published);
            }

            [Fact]
            public void Published_should_support_draft()
            {
                var settings = new Dictionary<string, object>()
                 {
                     {"published", "draft"}
                 };

                var post = new Post();

                post.SetHeaderSettings(settings);
                Assert.Equal(Published.Draft, post.Published);
            }

            [Fact]
            public void Published_should_support_private()
            {
                var settings = new Dictionary<string, object>()
                 {
                     {"published", "private"}
                 };

                var post = new Post();

                post.SetHeaderSettings(settings);
                Assert.Equal(Published.Private, post.Published);
            }
        }
    }
}