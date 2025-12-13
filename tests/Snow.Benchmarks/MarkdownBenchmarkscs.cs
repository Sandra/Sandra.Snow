using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Snow.Engine.Markdown;

namespace Snow.Benchmarks;

[MemoryDiagnoser]
[SimpleJob]
public class MarkdownBenchmarkscs
{
    private MarkdigMarkdownParser _parser = null!;
    private string _testContent = null!;

    [GlobalSetup] 
    public void Setup()
    {
        var logger = Substitute.For<ILogger<MarkdigMarkdownParser>>();
        _parser = new MarkdigMarkdownParser(MarkdownConfiguration.Default(), logger);
        _testContent = "# Test\n\n**Bold** text with [link](https://example.com).";
    }

    [Benchmark]
    public string ParseMarkdown() => _parser.Parse(_testContent);

    [GlobalCleanup]
    public void Cleanup() => _parser?.Dispose();
}
