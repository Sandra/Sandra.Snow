using FluentAssertions;
using Snow.Engine.Extensions;
using Xunit;

namespace Snow.Engine.Tests.Extensions;

public class SlugExtensionsTests
{
    [Theory]
    [InlineData("Hello World", "hello-world")]
    [InlineData("Testing & Development", "testing-and-development")]
    [InlineData("Multiple   Spaces", "multiple-spaces")]
    [InlineData("Special!@#$%^&*()Characters", "specialandcharacters")]
    [InlineData("Trailing---Dashes---", "trailing-dashes")]
    [InlineData("UPPERCASE TEXT", "uppercase-text")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    [InlineData("café", "cafe")]
    [InlineData("naïve résumé", "naive-resume")]
    public void ToUrlSlug_ShouldCreateValidSlug(string input, string expected)
    {
        // Act
        var result = input.ToUrlSlug();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("/", false)]
    [InlineData("/path/", false)]
    [InlineData("/path/to/page", false)]
    [InlineData("/path/to/file.html", true)]
    [InlineData("/images/photo.jpg", true)]
    [InlineData("/styles/main.css", true)]
    [InlineData("file.txt", true)]
    public void IsFileUrl_ShouldDetectFileUrls(string url, bool expected)
    {
        // Act
        var result = url.IsFileUrl();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("/path/to/file.html", "/path/to/file.html")]
    [InlineData("/path/to/directory", "/path/to/directory/")]
    [InlineData("/path/to/page", "/path/to/page/")]
    [InlineData("", "")]
    public void AppendSlashIfNecessary_ShouldAppendSlashForDirectories(string input, string expected)
    {
        // Act
        var result = input.AppendSlashIfNecessary();

        // Assert
        result.Should().Be(expected);
    }
}
