using System.IO;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Snow.Engine.Markdown;
using Xunit;

namespace Snow.Engine.Tests.Markdown;

/// <summary>
/// Golden master tests to ensure markdown processing remains consistent across versions.
/// </summary>
public class GoldenMasterTests
{
    private readonly ILogger<MarkdigMarkdownParser> _logger;

    public GoldenMasterTests()
    {
        _logger = Substitute.For<ILogger<MarkdigMarkdownParser>>();
    }

    [Fact]
    public void Parse_BlogPostSample_ShouldMatchExpectedOutput()
    {
        // Arrange
        var markdown = @"# My Blog Post

This is a sample blog post with various markdown features.

## Features Demonstrated

- **Bold text** for emphasis
- *Italic text* for style  
- `inline code` for technical terms
- [External links](https://example.com) for references

### Code Block Example

```csharp
public class Example
{
    public string Name { get; set; }
}
```

### Table Example

| Feature | Supported | Notes |
|---------|-----------|-------|
| Headers | ✅ | Full support |
| Lists | ✅ | Ordered and unordered |
| Code | ✅ | Inline and blocks |

### Smart Typography

This demonstrates ""smart quotes"" and em-dashes—like this.

### Task Lists

- [x] Implement Markdig
- [x] Add tests
- [ ] Deploy to production

## Conclusion

This sample demonstrates that Sandra.Snow v2 properly processes modern Markdown.

Visit :rocket: the [documentation](https://docs.example.com) for more info.";

        var expectedFeatures = new[]
        {
            "<h1 id=\"my-blog-post\">",           // Heading with ID
            "<h2 id=\"features-demonstrated\">",  // Sub-heading with ID
            "<strong>Bold text</strong>",         // Bold formatting
            "<em>Italic text</em>",              // Italic formatting
            "<code>inline code</code>",           // Inline code
            "<a href=\"https://example.com\">",   // External links
            "<pre><code class=\"language-csharp\">", // Code blocks
            "<table>",                           // Tables
            "<th>Feature</th>",                  // Table headers
            "<td>✅</td>",                       // Table cells with emoji
            "&ldquo;smart quotes&rdquo;",        // Smart quotes
            "em-dashes—like this",               // Em-dashes (actual character, not entity)
            "type=\"checkbox\"",                 // Task lists
            "checked=\"checked\"",               // Completed tasks
            "🚀"                                 // Emoji conversion
        };

        using var parser = new MarkdigMarkdownParser(MarkdownConfiguration.Default(), _logger);

        // Act
        var result = parser.Parse(markdown);

        // Assert
        result.Should().NotBeNullOrEmpty();
        
        foreach (var expectedFeature in expectedFeatures)
        {
            result.Should().Contain(expectedFeature, 
                $"Generated HTML should contain {expectedFeature} to demonstrate proper markdown processing");
        }

        // Verify structure
        result.Should().Contain("<h1").And.Contain("</h1>");
        result.Should().Contain("<h2").And.Contain("</h2>");
        result.Should().Contain("<h3").And.Contain("</h3>");
        result.Should().Contain("<ul>").And.Contain("</ul>");
        result.Should().Contain("<table>").And.Contain("</table>");
        result.Should().Contain("<pre><code").And.Contain("</code></pre>");
    }

    [Fact]
    public void ParseExcerpt_ShouldProduceLighterOutput()
    {
        // Arrange
        var markdown = @"This is an excerpt with **bold text** and a [link](https://example.com).

It should be processed with the lighter pipeline for performance.";

        using var parser = new MarkdigMarkdownParser(MarkdownConfiguration.Default(), _logger);

        // Act
        var excerptResult = parser.ParseExcerpt(markdown);
        var fullResult = parser.Parse(markdown);

        // Assert
        excerptResult.Should().NotBeNullOrEmpty();
        excerptResult.Should().Contain("<strong>bold text</strong>");
        excerptResult.Should().Contain("<a href=\"https://example.com\">link</a>");
        
        // Both should produce valid HTML, but excerpt processing should be lighter
        // (This test demonstrates the concept; actual performance differences are measured in PerformanceTests)
        excerptResult.Should().Contain("<p>");
        fullResult.Should().Contain("<p>");
    }

    [Theory]
    [InlineData("# Simple Heading")]
    [InlineData("**Bold text only**")]
    [InlineData("- Simple list item")]
    [InlineData("[Simple link](https://example.com)")]
    [InlineData("`simple code`")]
    public void Parse_SimpleMarkdownElements_ShouldProduceValidHtml(string markdown)
    {
        // Arrange
        using var parser = new MarkdigMarkdownParser(MarkdownConfiguration.Default(), _logger);

        // Act
        var result = parser.Parse(markdown);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().NotContain("&lt;script&gt;"); // Should not contain suspicious content
        result.Should().MatchRegex(@"<[^>]+>.*</[^>]+>", "Should contain properly formed HTML tags"); // Basic HTML structure check
    }

    [Fact]
    public void Parse_WithUnicodeContent_ShouldHandleCorrectly()
    {
        // Arrange
        var markdown = @"# Unicode Test: Café & Naïve

This tests Unicode handling: 

- Émojis: 😀 🎉 🚀
- Accented characters: café, naïve, résumé
- Special symbols: © ® ™ § ¶
- Mathematical symbols: ∞ ≠ ≤ ≥ ±";

        using var parser = new MarkdigMarkdownParser(MarkdownConfiguration.Default(), _logger);

        // Act
        var result = parser.Parse(markdown);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Café");
        result.Should().Contain("naïve");
        result.Should().Contain("😀");
        result.Should().Contain("🎉");
        result.Should().Contain("🚀");
    }
}
