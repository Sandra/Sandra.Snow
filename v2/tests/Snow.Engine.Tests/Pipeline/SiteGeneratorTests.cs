using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Snow.Abstractions;
using Snow.Engine.Configuration;
using Snow.Engine.Extensions;
using Snow.Engine.IO;
using Snow.Engine.Pipeline;
using Xunit;
using Xunit.Abstractions;

namespace Snow.Engine.Tests.Pipeline;

public class SiteGeneratorTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly string _testSiteDir;
    private readonly SiteSettings _settings;
    private readonly ServiceProvider _serviceProvider;
    private readonly SiteGenerator _generator;

    public SiteGeneratorTests(ITestOutputHelper output)
    {
        _output = output;
        _testSiteDir = Path.Combine(Path.GetTempPath(), "snow-site-" + Guid.NewGuid().ToString("N")[..8]);
        
        SetupTestSite();
        
        _settings = SiteSettings.CreateDefault(_testSiteDir);
        _settings.PostsDirectoryRaw = "_posts";
        _settings.PagesDirectoryRaw = "_pages";
        _settings.ThemesDirectory = "themes";
        _settings.Theme = "test";
        _settings.PostsOutputRaw = "output";
        _settings.RefreshPaths();

        var services = new ServiceCollection();
        services.AddSnow(_settings, enableDebugLogging: true);

        _serviceProvider = services.BuildServiceProvider();
        _generator = _serviceProvider.GetRequiredService<SiteGenerator>();
    }

    [Fact]
    public async Task GenerateAsync_WithValidContent_ShouldGenerateCompletesite()
    {
        // Arrange
        var progress = new TestProgressReporter();
        var generator = new SiteGenerator(
            _serviceProvider.GetRequiredService<IPostParser>(),
            _serviceProvider.GetRequiredService<IPageParser>(),
            _serviceProvider.GetRequiredService<ITemplateRenderer>(),
            _serviceProvider.GetRequiredService<IFileProvider>(),
            _settings,
            _serviceProvider.GetRequiredService<ILogger<SiteGenerator>>(),
            progress);

        // Act
        await generator.GenerateAsync();

        // Assert
        var outputDir = _settings.PostsOutput;
        Directory.Exists(outputDir).Should().BeTrue();
        
        // Should have generated post
        var postDir = Path.Combine(outputDir, "2023", "01", "01", "test-post");
        Directory.Exists(postDir).Should().BeTrue();
        File.Exists(Path.Combine(postDir, "index.html")).Should().BeTrue();

        // Should have generated page
        var aboutPath = Path.Combine(outputDir, "about", "index.html");
        File.Exists(aboutPath).Should().BeTrue();

        // Should have generated index
        File.Exists(Path.Combine(outputDir, "index.html")).Should().BeTrue();

        // Check progress reporting
        progress.FinalProgress.Should().NotBeNull();
        progress.FinalProgress!.IsComplete.Should().BeTrue();
        progress.FinalProgress.ErrorCount.Should().Be(0);
    }

    [Fact]
    public async Task GenerateAsync_WithCancellation_ShouldStopGracefully()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(10)); // Cancel very quickly

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _generator.GenerateAsync(cts.Token));
    }

    [Fact]
    public async Task GenerateAsync_WithMissingTemplate_ShouldContinueWithErrors()
    {
        // Arrange - Remove template files to cause errors
        var layoutsDir = Path.Combine(_testSiteDir, "themes", "test", "_layouts");
        if (Directory.Exists(layoutsDir))
        {
            Directory.Delete(layoutsDir, true);
        }

        var progress = new TestProgressReporter();
        var generator = new SiteGenerator(
            _serviceProvider.GetRequiredService<IPostParser>(),
            _serviceProvider.GetRequiredService<IPageParser>(),
            _serviceProvider.GetRequiredService<ITemplateRenderer>(),
            _serviceProvider.GetRequiredService<IFileProvider>(),
            _settings,
            _serviceProvider.GetRequiredService<ILogger<SiteGenerator>>(),
            progress);

        // Act
        await generator.GenerateAsync();

        // Assert - Should complete but with errors
        progress.FinalProgress.Should().NotBeNull();
        progress.FinalProgress!.ErrorCount.Should().BeGreaterThan(0);
    }

    private void SetupTestSite()
    {
        // Create directory structure
        Directory.CreateDirectory(_testSiteDir);
        Directory.CreateDirectory(Path.Combine(_testSiteDir, "_posts"));
        Directory.CreateDirectory(Path.Combine(_testSiteDir, "_pages"));
        Directory.CreateDirectory(Path.Combine(_testSiteDir, "themes", "test", "_layouts"));

        // Create test post
        File.WriteAllText(Path.Combine(_testSiteDir, "_posts", "2023-01-01-test-post.md"), @"---
title: Test Post
author: Test Author
layout: post
published: published
---

This is a test post content.

## Section

More content here.");

        // Create test page
        File.WriteAllText(Path.Combine(_testSiteDir, "_pages", "about.md"), @"---
title: About
layout: page
published: published
---

This is the about page.");

        // Create post layout
        File.WriteAllText(Path.Combine(_testSiteDir, "themes", "test", "_layouts", "post.cshtml"), @"<!DOCTYPE html>
<html>
<head>
    <title>@Model.Title</title>
</head>
<body>
    <h1>@Model.Title</h1>
    <div>@Html.Raw(Model.Content)</div>
    <p>By: @Model.Author</p>
</body>
</html>");

        // Create page layout
        File.WriteAllText(Path.Combine(_testSiteDir, "themes", "test", "_layouts", "page.cshtml"), @"<!DOCTYPE html>
<html>
<head>
    <title>@Model.Title</title>
</head>
<body>
    <h1>@Model.Title</h1>
    <div>@Html.Raw(Model.Content)</div>
</body>
</html>");

        // Create index template
        File.WriteAllText(Path.Combine(_testSiteDir, "themes", "test", "index.cshtml"), @"<!DOCTYPE html>
<html>
<head>
    <title>Blog Index</title>
</head>
<body>
    <h1>Recent Posts</h1>
    <p>Total posts: @Model.Posts.Count</p>
</body>
</html>");

        // Create snow config
        File.WriteAllText(Path.Combine(_testSiteDir, "snow.config.json"), @"{
    ""blogTitle"": ""Test Blog"",
    ""siteUrl"": ""https://test.example.com"",
    ""author"": ""Test Author"",
    ""email"": ""test@example.com"",
    ""processFiles"": [
        {""file"": ""index.cshtml""}
    ]
}");
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
        
        if (Directory.Exists(_testSiteDir))
        {
            try
            {
                Directory.Delete(_testSiteDir, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}

/// <summary>
/// Test implementation of progress reporting.
/// </summary>
public class TestProgressReporter : IProgress<GeneratorProgress>
{
    private readonly List<GeneratorProgress> _progressReports = [];

    public GeneratorProgress? FinalProgress { get; private set; }

    public IReadOnlyList<GeneratorProgress> AllProgress => _progressReports;

    public void Report(GeneratorProgress value)
    {
        _progressReports.Add(value);
        FinalProgress = value;
    }
}
