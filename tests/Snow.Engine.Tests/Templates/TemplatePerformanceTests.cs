using System;
using System.Diagnostics;
using System.IO;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Snow.Engine.Configuration;
using Snow.Engine.IO;
using Snow.Engine.Templates;
using Snow.Engine.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace Snow.Engine.Tests.Templates;

public class TemplatePerformanceTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly ILogger<SimpleTemplateRenderer> _logger;
    private readonly SiteSettings _settings;
    private readonly FileSystemProvider _fileProvider;
    private readonly SimpleTemplateRenderer _renderer;
    private readonly string _testThemeDir;

    public TemplatePerformanceTests(ITestOutputHelper output)
    {
        _output = output;
        _logger = Substitute.For<ILogger<SimpleTemplateRenderer>>();
        
        _testThemeDir = Path.Combine(Path.GetTempPath(), "snow-perf-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_testThemeDir);
        
        _settings = SiteSettings.CreateDefault(_testThemeDir);
        _settings.ThemesDirectory = "themes";
        _settings.Theme = "test";
        
        _fileProvider = new FileSystemProvider();
        _renderer = new SimpleTemplateRenderer(_fileProvider, _settings, _logger);
        
        SetupPerformanceTemplates();
    }

    [Fact]
    public void Render_MultipleTemplates_ShouldBeCached()
    {
        // Arrange
        var model = new TemplateViewModel
        {
            Title = "Performance Test",
            Content = "Test content for caching"
        };

        // Act - First render (loads from disk)
        var firstStopwatch = Stopwatch.StartNew();
        var firstResult = _renderer.Render("performance", model);
        firstStopwatch.Stop();

        // Act - Second render (should be cached)
        var secondStopwatch = Stopwatch.StartNew();
        var secondResult = _renderer.Render("performance", model);
        secondStopwatch.Stop();

        // Assert
        firstResult.Should().Be(secondResult);
        _output.WriteLine($"First render: {firstStopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Second render (cached): {secondStopwatch.ElapsedMilliseconds}ms");
        
        // Second render should be significantly faster due to caching
        secondStopwatch.ElapsedMilliseconds.Should().BeLessThanOrEqualTo(
            firstStopwatch.ElapsedMilliseconds, 
            "Cached rendering should be faster or equal");
    }

    [Fact]
    public void Render_ManyTemplatesConcurrently_ShouldBeThreadSafe()
    {
        // Arrange
        const int concurrentRenders = 50;
        var models = new TemplateViewModel[concurrentRenders];
        
        for (int i = 0; i < concurrentRenders; i++)
        {
            models[i] = new TemplateViewModel
            {
                Title = $"Concurrent Test {i}",
                Content = $"Content for test {i}"
            };
        }

        // Act
        var results = new string[concurrentRenders];
        var stopwatch = Stopwatch.StartNew();
        
        System.Threading.Tasks.Parallel.For(0, concurrentRenders, i =>
        {
            results[i] = _renderer.Render("performance", models[i]);
        });
        
        stopwatch.Stop();

        // Assert
        _output.WriteLine($"Rendered {concurrentRenders} templates concurrently in {stopwatch.ElapsedMilliseconds}ms");
        
        foreach (var result in results)
        {
            result.Should().NotBeNullOrEmpty();
            result.Should().Contain("Performance Template");
        }
        
        // Should complete in reasonable time
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000, 
            "Concurrent template rendering should complete quickly");
    }

    [Theory]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    public void Render_MultipleSequentialRenders_ShouldScaleLinearly(int renderCount)
    {
        // Arrange
        var model = new TemplateViewModel
        {
            Title = "Scale Test",
            Content = "Content for scaling test"
        };

        // Warm up
        _renderer.Render("performance", model);

        // Act
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < renderCount; i++)
        {
            _renderer.Render("performance", model);
        }
        stopwatch.Stop();

        // Assert
        var timePerRender = (double)stopwatch.ElapsedMilliseconds / renderCount;
        _output.WriteLine($"{renderCount} renders in {stopwatch.ElapsedMilliseconds}ms ({timePerRender:F2}ms per render)");
        
        timePerRender.Should().BeLessThan(10.0, 
            "Each cached template render should be very fast");
    }

    private void SetupPerformanceTemplates()
    {
        var testThemeDir = Path.Combine(_testThemeDir, "themes", "test");
        Directory.CreateDirectory(testThemeDir);

        // Performance test template
        File.WriteAllText(Path.Combine(testThemeDir, "performance.cshtml"), @"
<html>
<head>
    <title>@Model.Title</title>
</head>
<body>
    <h1>Performance Template</h1>
    <h2>@Model.Title</h2>
    <div>@Model.Content</div>
</body>
</html>
");
    }

    public void Dispose()
    {
        if (Directory.Exists(_testThemeDir))
        {
            Directory.Delete(_testThemeDir, true);
        }
    }
}
