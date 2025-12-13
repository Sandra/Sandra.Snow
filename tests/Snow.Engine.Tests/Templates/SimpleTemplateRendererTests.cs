using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Snow.Abstractions;
using Snow.Engine.Configuration;
using Snow.Engine.IO;
using Snow.Engine.Templates;
using Snow.Engine.ViewModels;
using Xunit;

namespace Snow.Engine.Tests.Templates;

public class SimpleTemplateRendererTests : IDisposable
{
    private readonly ILogger<SimpleTemplateRenderer> _logger;
    private readonly SiteSettings _settings;
    private readonly FileSystemProvider _fileProvider;
    private readonly SimpleTemplateRenderer _renderer;
    private readonly string _testThemeDir;

    public SimpleTemplateRendererTests()
    {
        _logger = Substitute.For<ILogger<SimpleTemplateRenderer>>();
        
        // Create a test theme directory
        _testThemeDir = Path.Combine(Path.GetTempPath(), "snow-test-" + System.Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_testThemeDir);
        
        _settings = SiteSettings.CreateDefault(_testThemeDir);
        _settings.ThemesDirectory = "themes";
        _settings.Theme = "test";
        
        _fileProvider = new FileSystemProvider();
        _renderer = new SimpleTemplateRenderer(_fileProvider, _settings, _logger);
        
        SetupTestTemplates();
    }

    [Fact]
    public void TemplateExists_WithValidTemplate_ShouldReturnTrue()
    {
        // Act
        var exists = _renderer.TemplateExists("simple");

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public void TemplateExists_WithNonexistentTemplate_ShouldReturnFalse()
    {
        // Act
        var exists = _renderer.TemplateExists("nonexistent");

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public void TemplateExists_WithEmptyPath_ShouldReturnFalse()
    {
        // Act
        var exists = _renderer.TemplateExists("");

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public void Render_WithSimpleTemplate_ShouldReplaceModelProperties()
    {
        // Arrange
        var model = new TemplateViewModel
        {
            Title = "Test Post",
            Content = "This is test content",
            Author = "Test Author"
        };

        // Act
        var result = _renderer.Render("simple", model);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Test Post");
        result.Should().Contain("This is test content");
        result.Should().Contain("Test Author");
    }

    [Fact]
    public void Render_WithLayoutTemplate_ShouldApplyLayout()
    {
        // Arrange
        var model = new TemplateViewModel
        {
            Title = "Test Post with Layout",
            Content = "Content with layout"
        };

        // Act
        var result = _renderer.Render("with-layout", model);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Test Layout");
        result.Should().Contain("Test Post with Layout");
        result.Should().Contain("Content with layout");
    }

    [Fact]
    public void Render_WithRawContent_ShouldNotEscape()
    {
        // Arrange
        var model = new TemplateViewModel
        {
            Content = "<strong>Bold HTML</strong>"
        };

        // Act
        var result = _renderer.Render("raw-content", model);

        // Assert
        result.Should().Contain("<strong>Bold HTML</strong>");
        result.Should().NotContain("&lt;strong&gt;");
    }

    [Fact]
    public void Render_WithNonexistentTemplate_ShouldReturnErrorTemplate()
    {
        // Arrange
        var model = new TemplateViewModel { Title = "Test" };

        // Act
        var result = _renderer.Render("nonexistent", model);

        // Assert
        result.Should().Contain("Template Error");
        result.Should().Contain("nonexistent");
    }

    [Fact]
    public void Render_WithTemplateNotInCurrentTheme_ShouldReturnError()
    {
        // Arrange - template exists in default theme but not in test theme
        // Note: Theme fallback is not currently implemented; this test verifies current behavior
        
        // Act
        var result = _renderer.Render("fallback-only", new TemplateViewModel { Title = "Fallback Test" });

        // Assert - Should return error template since fallback-only.cshtml is not in test theme
        result.Should().Contain("Template Error");
        result.Should().Contain("fallback-only");
    }

    private void SetupTestTemplates()
    {
        // Create test theme directory structure
        var testThemeDir = Path.Combine(_testThemeDir, "themes", "test");
        var defaultThemeDir = Path.Combine(_testThemeDir, "themes", "default");
        var layoutsDir = Path.Combine(testThemeDir, "_layouts");
        var defaultLayoutsDir = Path.Combine(defaultThemeDir, "_layouts");
        
        Directory.CreateDirectory(testThemeDir);
        Directory.CreateDirectory(defaultThemeDir);
        Directory.CreateDirectory(layoutsDir);
        Directory.CreateDirectory(defaultLayoutsDir);

        // Simple template
        File.WriteAllText(Path.Combine(testThemeDir, "simple.cshtml"), @"
<h1>@Model.Title</h1>
<div>@Model.Content</div>
<p>By: @Model.Author</p>
");

        // Template with layout
        File.WriteAllText(Path.Combine(layoutsDir, "main.cshtml"), @"
<!DOCTYPE html>
<html>
<head><title>Test Layout</title></head>
<body>
@RenderBody()
</body>
</html>
");

        File.WriteAllText(Path.Combine(testThemeDir, "with-layout.cshtml"), @"
@{ Layout = ""main""; }
<h1>@Model.Title</h1>
<div>@Model.Content</div>
");

        // Template with raw content
        File.WriteAllText(Path.Combine(testThemeDir, "raw-content.cshtml"), @"
<div>@Html.Raw(Model.Content)</div>
");

        // Template in default theme only (for fallback testing)
        File.WriteAllText(Path.Combine(defaultThemeDir, "fallback-only.cshtml"), @"
<div>Default Theme Template</div>
<h1>@Model.Title</h1>
");
    }

    public void Dispose()
    {
        // Clean up test directory
        if (Directory.Exists(_testThemeDir))
        {
            Directory.Delete(_testThemeDir, true);
        }
    }
}
