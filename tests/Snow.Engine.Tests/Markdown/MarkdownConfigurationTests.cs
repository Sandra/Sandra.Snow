using System.Linq;
using FluentAssertions;
using Snow.Engine.Markdown;
using Xunit;

namespace Snow.Engine.Tests.Markdown;

public class MarkdownConfigurationTests
{
    [Fact]
    public void Default_ShouldCreateValidConfiguration()
    {
        // Act
        var config = MarkdownConfiguration.Default();

        // Assert
        config.Should().NotBeNull();
        config.Extensions.Should().NotBeEmpty();
        config.EnableAdvancedExtensions.Should().BeTrue();
        config.SanitizeHtml.Should().BeTrue();
        config.HeadingIds.Should().Be(HeadingIdStyle.GitHub);
        config.HardLineBreaks.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithValidConfiguration_ShouldReturnNoErrors()
    {
        // Arrange
        var config = MarkdownConfiguration.Default();

        // Act
        var errors = config.Validate();

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithInvalidExtension_ShouldReturnError()
    {
        // Arrange
        var config = new MarkdownConfiguration()
            .SetExtensions("invalid-extension", "emoji");

        // Act
        var errors = config.Validate().ToList();

        // Assert
        errors.Should().NotBeEmpty();
        errors[0].Should().Contain("Invalid markdown extensions");
        errors[0].Should().Contain("invalid-extension");
    }

    [Theory]
    [InlineData("emoji")]
    [InlineData("smartypants")]
    [InlineData("tables")]
    [InlineData("tasklists")]
    [InlineData("autolinks")]
    [InlineData("math")]
    [InlineData("footnotes")]
    [InlineData("abbreviations")]
    [InlineData("definitionlists")]
    [InlineData("figures")]
    [InlineData("customcontainers")]
    [InlineData("medialinks")]
    [InlineData("genericattributes")]
    public void Validate_WithValidExtensions_ShouldReturnNoErrors(string extension)
    {
        // Arrange
        var config = new MarkdownConfiguration()
            .SetExtensions(extension);

        // Act
        var errors = config.Validate();

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void WithExtensions_ShouldAddToExistingExtensions()
    {
        // Arrange
        var config = MarkdownConfiguration.Default();
        var initialCount = config.Extensions.Count;

        // Act
        config.WithExtensions("math", "footnotes");

        // Assert
        config.Extensions.Should().HaveCount(initialCount + 2);
        config.Extensions.Should().Contain("math");
        config.Extensions.Should().Contain("footnotes");
    }

    [Fact]
    public void WithoutExtensions_ShouldRemoveSpecifiedExtensions()
    {
        // Arrange
        var config = MarkdownConfiguration.Default()
            .WithExtensions("math", "footnotes");

        // Act
        config.WithoutExtensions("math");

        // Assert
        config.Extensions.Should().Contain("footnotes");
        config.Extensions.Should().NotContain("math");
    }

    [Fact]
    public void SetExtensions_ShouldReplaceAllExtensions()
    {
        // Arrange
        var config = MarkdownConfiguration.Default();

        // Act
        config.SetExtensions("math", "footnotes");

        // Assert
        config.Extensions.Should().HaveCount(2);
        config.Extensions.Should().Contain("math");
        config.Extensions.Should().Contain("footnotes");
        config.Extensions.Should().NotContain("emoji"); // Default extension should be gone
    }

    [Fact]
    public void SyntaxHighlightingConfiguration_ShouldHaveDefaults()
    {
        // Act
        var config = new SyntaxHighlightingConfiguration();

        // Assert
        config.Enabled.Should().BeFalse();
        config.Theme.Should().Be("prism-dark");
        config.LineNumbers.Should().BeFalse();
    }

    [Theory]
    [InlineData(HeadingIdStyle.None)]
    [InlineData(HeadingIdStyle.GitHub)]
    [InlineData(HeadingIdStyle.Legacy)]
    public void HeadingIdStyle_AllValuesShouldBeValid(HeadingIdStyle style)
    {
        // Arrange
        var config = new MarkdownConfiguration
        {
            HeadingIds = style
        };

        // Act
        var errors = config.Validate();

        // Assert
        errors.Should().BeEmpty();
    }
}
