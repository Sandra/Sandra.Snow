using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Snow.Engine.Markdown;
using Xunit;

namespace Snow.Engine.Tests.Markdown;

public class MarkdigMarkdownParserTests : IDisposable
{
    private readonly ILogger<MarkdigMarkdownParser> _logger;
    private readonly MarkdigMarkdownParser _parser;

    public MarkdigMarkdownParserTests()
    {
        _logger = Substitute.For<ILogger<MarkdigMarkdownParser>>();
        _parser = new MarkdigMarkdownParser(MarkdownConfiguration.Default(), _logger);
    }

    [Fact]
    public void Parse_WithEmptyInput_ShouldReturnEmpty()
    {
        // Act
        var result = _parser.Parse("");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Parse_WithNull_ShouldReturnEmpty()
    {
        // Act
        var result = _parser.Parse(null!);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Parse_WithSimpleText_ShouldWrapInParagraph()
    {
        // Arrange
        var markdown = "Hello world";

        // Act
        var result = _parser.Parse(markdown);

        // Assert
        result.Should().Contain("<p>Hello world</p>");
    }

    [Fact]
    public void Parse_WithHeading_ShouldGenerateHeadingWithId()
    {
        // Arrange
        var markdown = "# My Heading";

        // Act
        var result = _parser.Parse(markdown);

        // Assert
        result.Should().Contain("<h1 id=\"my-heading\">My Heading</h1>");
    }

    [Fact]
    public void Parse_WithEmoji_ShouldConvertEmoji()
    {
        // Arrange
        var markdown = "Hello :rocket: world";

        // Act
        var result = _parser.Parse(markdown);

        // Assert
        result.Should().Contain("🚀");
    }

    [Fact]
    public void Parse_WithTable_ShouldGenerateTable()
    {
        // Arrange
        var markdown = @"| Header 1 | Header 2 |
| -------- | -------- |
| Cell 1   | Cell 2   |";

        // Act
        var result = _parser.Parse(markdown);

        // Assert
        result.Should().Contain("<table>");
        result.Should().Contain("<th>Header 1</th>");
        result.Should().Contain("<td>Cell 1</td>");
    }

    [Fact]
    public void Parse_WithTaskList_ShouldGenerateTaskList()
    {
        // Arrange
        var markdown = @"- [x] Completed task
- [ ] Incomplete task";

        // Act
        var result = _parser.Parse(markdown);

        // Assert
        result.Should().Contain("type=\"checkbox\"");
        result.Should().Contain("checked=\"checked\"");
    }

    [Fact]
    public void Parse_WithSmartyPants_ShouldConvertQuotes()
    {
        // Arrange
        var markdown = "\"Hello world\"";

        // Act
        var result = _parser.Parse(markdown);

        // Assert
        // SmartyPants converts straight quotes to HTML entities
        result.Should().Contain("&ldquo;").And.Contain("&rdquo;");
    }

    [Fact]
    public void Parse_WithAutolinks_ShouldConvertUrls()
    {
        // Arrange
        var markdown = "Visit https://example.com for more info";

        // Act
        var result = _parser.Parse(markdown);

        // Assert
        result.Should().Contain("<a href=\"https://example.com\">https://example.com</a>");
    }

    [Fact]
    public void Parse_WithRawHtml_ShouldSanitizeByDefault()
    {
        // Arrange
        var markdown = "Hello <script>alert('xss')</script> world";

        // Act
        var result = _parser.Parse(markdown);

        // Assert
        result.Should().NotContain("<script>");
        // HTML is escaped, so alert will be present as text but not executable
        result.Should().Contain("&lt;script&gt;");
    }

    [Theory]
    [InlineData("# Heading", "<h1 id=\"heading\">Heading</h1>")]
    [InlineData("## Sub Heading", "<h2 id=\"sub-heading\">Sub Heading</h2>")]
    [InlineData("### Third Level", "<h3 id=\"third-level\">Third Level</h3>")]
    public void Parse_WithHeadings_ShouldGenerateCorrectIds(string markdown, string expectedHtml)
    {
        // Act
        var result = _parser.Parse(markdown);

        // Assert
        result.Should().Contain(expectedHtml);
    }

    [Fact]
    public async Task ParseAsync_WithSimpleText_ShouldReturnSameAsSync()
    {
        // Arrange
        var markdown = "Hello world";

        // Act
        var syncResult = _parser.Parse(markdown);
        var asyncResult = await _parser.ParseAsync(markdown);

        // Assert
        asyncResult.Should().Be(syncResult);
    }

    [Fact]
    public void ParseExcerpt_WithSimpleText_ShouldUseLightweightPipeline()
    {
        // Arrange
        var markdown = "Hello world";

        // Act
        var result = _parser.ParseExcerpt(markdown);

        // Assert
        result.Should().Contain("<p>Hello world</p>");
    }

    [Fact]
    public void ParseExcerpt_WithRawHtml_ShouldAlwaysSanitize()
    {
        // Arrange
        var markdown = "Hello <script>alert('xss')</script> world";

        // Act
        var result = _parser.ParseExcerpt(markdown);

        // Assert
        result.Should().NotContain("<script>");
    }

    [Fact]
    public async Task ParseExcerptAsync_WithSimpleText_ShouldReturnSameAsSync()
    {
        // Arrange
        var markdown = "Hello world";

        // Act
        var syncResult = _parser.ParseExcerpt(markdown);
        var asyncResult = await _parser.ParseExcerptAsync(markdown);

        // Assert
        asyncResult.Should().Be(syncResult);
    }

    [Fact]
    public void Configuration_ShouldReturnProvidedConfiguration()
    {
        // Act & Assert
        _parser.Configuration.Should().NotBeNull();
        _parser.Configuration.EnableAdvancedExtensions.Should().BeTrue();
    }

    [Fact]
    public void CreateDefault_ShouldCreateParserWithDefaultConfiguration()
    {
        // Act
        using var defaultParser = MarkdigMarkdownParser.CreateDefault(_logger);

        // Assert
        defaultParser.Should().NotBeNull();
        defaultParser.Configuration.Should().NotBeNull();
    }

    public void Dispose()
    {
        _parser?.Dispose();
    }
}
