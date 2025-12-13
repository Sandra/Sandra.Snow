using System;
using System.Diagnostics;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Snow.Engine.Markdown;

using Xunit;
using Xunit.Abstractions;

namespace Snow.Engine.Tests.Markdown;

public class MarkdownPerformanceTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly ILogger<MarkdigMarkdownParser> _logger;
    private readonly MarkdigMarkdownParser _markdigParser;

    public MarkdownPerformanceTests(ITestOutputHelper output)
    {
        _output = output;
        _logger = Substitute.For<ILogger<MarkdigMarkdownParser>>();
        _markdigParser = new MarkdigMarkdownParser(MarkdownConfiguration.Default(), _logger);
    }

    [Fact]
    public void MarkdigParser_ShouldProcessLargeContentQuickly()
    {
        // Arrange
        var largeMarkdown = GenerateLargeMarkdownContent(1000); // 1000 lines

        // Warm up JIT
        _markdigParser.Parse("# Warmup");

        // Act - Measure Markdig performance
        var stopwatch = Stopwatch.StartNew();
        var result = _markdigParser.Parse(largeMarkdown);
        stopwatch.Stop();

        // Assert
        _output.WriteLine($"Markdig processed {largeMarkdown.Length} characters in {stopwatch.ElapsedMilliseconds}ms");
        
        // Markdig should produce proper HTML
        result.Should().Contain("<h1");
        result.Should().Contain("<ul>");
        result.Should().Contain("<strong>");
        result.Should().Contain("<table>");
        
        // Should be fast for static site generation
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, "Markdig should process large content quickly");
        
        // Performance metric
        var charactersPerMs = largeMarkdown.Length / (double)Math.Max(1, stopwatch.ElapsedMilliseconds);
        _output.WriteLine($"Performance: {charactersPerMs:F2} characters/ms");
        charactersPerMs.Should().BeGreaterThan(100, "Should process at least 100 characters per millisecond");
    }

    [Fact]
    public void MarkdigParser_ExcerptPipeline_ShouldBeFasterThanFullPipeline()
    {
        // Arrange
        var markdown = GenerateMarkdownContent(100);

        // Warm up
        _markdigParser.Parse(markdown);
        _markdigParser.ParseExcerpt(markdown);

        // Act - Measure full pipeline
        var fullStopwatch = Stopwatch.StartNew();
        for (int i = 0; i < 100; i++)
        {
            _markdigParser.Parse(markdown);
        }
        fullStopwatch.Stop();

        // Act - Measure excerpt pipeline
        var excerptStopwatch = Stopwatch.StartNew();
        for (int i = 0; i < 100; i++)
        {
            _markdigParser.ParseExcerpt(markdown);
        }
        excerptStopwatch.Stop();

        // Assert
        _output.WriteLine($"Full pipeline (100 iterations): {fullStopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Excerpt pipeline (100 iterations): {excerptStopwatch.ElapsedMilliseconds}ms");
        
        // Excerpt pipeline should be faster or comparable (it's optimized for lighter processing)
        excerptStopwatch.ElapsedMilliseconds.Should().BeLessThanOrEqualTo(
            (long)(fullStopwatch.ElapsedMilliseconds * 1.2), // Allow 20% tolerance
            "Excerpt pipeline should be faster or comparable to full pipeline");
    }

    [Fact]
    public void MarkdigParser_ShouldHandleParallelProcessing()
    {
        // Arrange
        var markdowns = new string[10];
        for (int i = 0; i < markdowns.Length; i++)
        {
            markdowns[i] = GenerateMarkdownContent(50);
        }

        // Act & Assert - Should not throw when processing in parallel
        var results = new string[markdowns.Length];
        System.Threading.Tasks.Parallel.For(0, markdowns.Length, i =>
        {
            results[i] = _markdigParser.Parse(markdowns[i]);
        });

        // Verify all results are valid
        foreach (var result in results)
        {
            result.Should().NotBeNullOrEmpty();
            result.Should().Contain("<p>");
        }
    }

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(500)]
    public void MarkdigParser_ShouldScaleLinearlyWithContentSize(int lineCount)
    {
        // Arrange
        var markdown = GenerateMarkdownContent(lineCount);

        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = _markdigParser.Parse(markdown);
        stopwatch.Stop();

        // Assert
        _output.WriteLine($"{lineCount} lines processed in {stopwatch.ElapsedMilliseconds}ms");
        
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("<p>");
        
        // Should be reasonably fast for typical static site content
        // Note: First run may be slower due to JIT compilation
        var timePerLine = (double)stopwatch.ElapsedMilliseconds / lineCount;
        timePerLine.Should().BeLessThan(10.0, "Should process reasonably fast (allow for JIT and initial setup)");
    }

    private static string GenerateMarkdownContent(int lineCount)
    {
        var sb = new StringBuilder();
        var random = new Random(42); // Fixed seed for reproducible tests
        
        for (int i = 0; i < lineCount; i++)
        {
            var lineType = random.Next(5);
            switch (lineType)
            {
                case 0:
                    sb.AppendLine($"# Heading {i}");
                    break;
                case 1:
                    sb.AppendLine($"This is paragraph {i} with some **bold** text and *italic* text.");
                    break;
                case 2:
                    sb.AppendLine($"- List item {i}");
                    break;
                case 3:
                    sb.AppendLine($"[Link {i}](https://example.com/{i})");
                    break;
                case 4:
                    sb.AppendLine($"`inline code {i}` and regular text");
                    break;
            }
        }
        
        return sb.ToString();
    }

    private static string GenerateLargeMarkdownContent(int lineCount)
    {
        var sb = new StringBuilder();
        
        // Add various markdown elements for more realistic testing
        sb.AppendLine("# Large Document Performance Test");
        sb.AppendLine();
        sb.AppendLine("This is a large document to test Markdig performance.");
        sb.AppendLine();
        
        for (int i = 0; i < lineCount; i++)
        {
            sb.AppendLine($"## Section {i}");
            sb.AppendLine();
            sb.AppendLine($"This is paragraph {i} with **bold text**, *italic text*, and [a link](https://example.com/{i}).");
            sb.AppendLine();
            sb.AppendLine($"- Item 1 for section {i}");
            sb.AppendLine($"- Item 2 for section {i} with `inline code`");
            sb.AppendLine($"- Item 3 for section {i}");
            sb.AppendLine();
            
            if (i % 10 == 0)
            {
                sb.AppendLine("| Column 1 | Column 2 | Column 3 |");
                sb.AppendLine("|----------|----------|----------|");
                sb.AppendLine($"| Data {i} | Value {i} | Result {i} |");
                sb.AppendLine();
            }
        }
        
        return sb.ToString();
    }

    public void Dispose()
    {
        _markdigParser?.Dispose();
    }
}
