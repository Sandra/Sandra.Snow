using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Snow.Engine.Markdown;
using Xunit;
using Xunit.Abstractions;

namespace Snow.Engine.Tests.Markdown;

public class MarkdownConcurrencyTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly ILogger<MarkdigMarkdownParser> _logger;
    private readonly MarkdigMarkdownParser _parser;

    public MarkdownConcurrencyTests(ITestOutputHelper output)
    {
        _output = output;
        _logger = Substitute.For<ILogger<MarkdigMarkdownParser>>();
        _parser = new MarkdigMarkdownParser(MarkdownConfiguration.Default(), _logger);
    }

    [Fact]
    public async Task ParseAsync_ConcurrentAccess_ShouldBeThreadSafe()
    {
        // Arrange
        const int concurrentOperations = 50;
        const int operationsPerThread = 10;
        
        var markdownSamples = GenerateMarkdownSamples(concurrentOperations);
        var results = new ConcurrentBag<string>();
        var exceptions = new ConcurrentBag<Exception>();

        // Act - Parse many markdown documents concurrently
        var tasks = Enumerable.Range(0, concurrentOperations)
            .Select(async i =>
            {
                try
                {
                    for (int j = 0; j < operationsPerThread; j++)
                    {
                        var markdown = markdownSamples[i % markdownSamples.Length];
                        var result = await _parser.ParseAsync(markdown);
                        results.Add(result);
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            });

        await Task.WhenAll(tasks);

        // Assert
        exceptions.Should().BeEmpty("No exceptions should occur during concurrent parsing");
        results.Should().HaveCount(concurrentOperations * operationsPerThread);
        
        // All results should be valid HTML
        foreach (var result in results)
        {
            result.Should().NotBeNullOrEmpty();
            result.Should().Contain("<");
        }

        _output.WriteLine($"Successfully processed {results.Count} markdown documents concurrently");
    }

    [Fact]
    public void Parse_ConcurrentAccess_ShouldBeThreadSafe()
    {
        // Arrange
        const int concurrentOperations = 50;
        var markdownSamples = GenerateMarkdownSamples(concurrentOperations);
        var results = new ConcurrentBag<string>();
        var exceptions = new ConcurrentBag<Exception>();

        // Act - Parse many markdown documents using Parallel.ForEach
        Parallel.ForEach(Enumerable.Range(0, concurrentOperations), i =>
        {
            try
            {
                var markdown = markdownSamples[i % markdownSamples.Length];
                var result = _parser.Parse(markdown);
                results.Add(result);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        });

        // Assert
        exceptions.Should().BeEmpty("No exceptions should occur during concurrent parsing");
        results.Should().HaveCount(concurrentOperations);
        
        // All results should be valid HTML
        foreach (var result in results)
        {
            result.Should().NotBeNullOrEmpty();
            result.Should().Contain("<");
        }

        _output.WriteLine($"Successfully processed {results.Count} markdown documents in parallel");
    }

    [Fact]
    public async Task ParseExcerptAsync_ConcurrentAccess_ShouldBeThreadSafe()
    {
        // Arrange
        const int concurrentOperations = 30;
        var markdownSamples = GenerateMarkdownSamples(concurrentOperations);
        var fullResults = new ConcurrentBag<string>();
        var excerptResults = new ConcurrentBag<string>();
        var exceptions = new ConcurrentBag<Exception>();

        // Act - Mix full and excerpt parsing concurrently
        var tasks = Enumerable.Range(0, concurrentOperations)
            .Select(async i =>
            {
                try
                {
                    var markdown = markdownSamples[i % markdownSamples.Length];
                    
                    if (i % 2 == 0)
                    {
                        var result = await _parser.ParseAsync(markdown);
                        fullResults.Add(result);
                    }
                    else
                    {
                        var result = await _parser.ParseExcerptAsync(markdown);
                        excerptResults.Add(result);
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            });

        await Task.WhenAll(tasks);

        // Assert
        exceptions.Should().BeEmpty("No exceptions should occur during mixed concurrent parsing");
        (fullResults.Count + excerptResults.Count).Should().Be(concurrentOperations);

        _output.WriteLine($"Successfully mixed full ({fullResults.Count}) and excerpt ({excerptResults.Count}) parsing concurrently");
    }

    private static string[] GenerateMarkdownSamples(int count)
    {
        var samples = new string[Math.Min(count, 10)]; // Limit to 10 unique samples, reuse as needed
        var random = new Random(42); // Fixed seed for reproducible tests

        for (int i = 0; i < samples.Length; i++)
        {
            var content = $@"# Sample Document {i}

This is sample markdown document number {i} for concurrency testing.

## Features in Document {i}

- **Bold text** item {i}
- *Italic text* item {i}
- `Code snippet {i}`
- [Link {i}](https://example.com/{i})

### Code Block {i}

```csharp
public class Sample{i}
{{
    public string Name {{ get; set; }} = ""Sample {i}"";
}}
```

### Table {i}

| Column 1 | Column 2 | Column 3 |
|----------|----------|----------|
| Data {i} | Value {i} | Result {i} |

> This is a blockquote in document {i}.

---

End of document {i}.";

            samples[i] = content;
        }

        return samples;
    }

    public void Dispose()
    {
        _parser?.Dispose();
    }
}
