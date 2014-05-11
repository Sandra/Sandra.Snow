namespace Snow.Tests
{
    using Models;
    using System.Collections.Generic;
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
    }
}