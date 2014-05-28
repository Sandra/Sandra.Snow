namespace Snow.Tests
{
    using Models;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class CategoriesPageTests
    {
        [Fact]
        public void Should_be_case_insensitive()
        {
            // Arrange
            var categories = new List<string> { ".net", ".Net" };
            var posts = new List<Post>
            {
                new Post {Categories = categories}
            };

            // Act
            var result = CategoriesPage.Create(posts);

            // Assert
            Assert.Equal(1, result.Count);
            Assert.Equal(".Net", result[0].Name);
        }

        [Fact]
        public void Should_sort_categories_ascending()
        {
            // Arrange
            var post = new Post();
            var settings = new Dictionary<string, object>
            {
                {"categories", "b, A, C, .Net"}
            };

            // Act
            post.SetHeaderSettings(settings);

            // Assert
            var categories = post.Categories.ToArray();
            Assert.Equal(".Net", categories[0]);
            Assert.Equal("A", categories[1]);
            Assert.Equal("b", categories[2]);
            Assert.Equal("C", categories[3]);
        }
    }
}