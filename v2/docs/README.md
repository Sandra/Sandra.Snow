# Sandra.Snow v2 Documentation

Welcome to Sandra.Snow v2 - a modern static site generator inspired by Jekyll, built with .NET 9.

## 🚀 Quick Start

### Installation
```bash
# Install as global .NET tool
dotnet tool install --global Sandra.Snow.Cli

# Verify installation
sandra --version
```

### Create New Site
```bash
# Create new site (coming in future release)
sandra new my-blog

# For now, create directory structure manually:
mkdir my-blog
cd my-blog
mkdir _posts _pages themes/default/_layouts
```

### Create First Post
```markdown
<!-- _posts/2024-01-01-hello-world.md -->
---
title: Hello World
author: Your Name
layout: post
published: published
---

Welcome to Sandra.Snow v2!

## Features

- **Fast**: 5-10x faster builds than v1
- **Modern**: .NET 9 with cross-platform support
- **Live Reload**: Instant browser updates during development
- **Enhanced Markdown**: Tables, emoji, task lists, and more

Happy blogging! 🎉
```

### Create Configuration
```json
{
    "blogTitle": "My Awesome Blog",
    "siteUrl": "https://myblog.com",
    "author": "Your Name",
    "email": "you@example.com",
    "theme": "default",
    "processFiles": [
        {"file": "index.cshtml"}
    ]
}
```

### Build and Serve
```bash
# Build static site
sandra build

# Development with live reload
sandra serve --watch
```

## 📚 Documentation Structure

### Migration
- [Migration Overview](migration/overview.md) - Complete migration guide from v1
- [Breaking Changes](migration/breaking-changes.md) - All breaking changes with examples
- [Template Conversion](migration/template-conversion.md) - Template syntax migration

### Tutorials
- [Creating Your First Site](tutorials/first-site.md) - Step-by-step tutorial
- [Blog Setup](tutorials/blog-setup.md) - Complete blog configuration
- [Advanced Features](tutorials/advanced-features.md) - Series, categories, custom layouts

### Theme Development
- [Theme Development Guide](theme-development/intro.md) - Creating custom themes
- [Template Syntax](theme-development/template-syntax.md) - Template system reference
- [Examples](theme-development/examples/) - Complete theme examples

### API Reference
- [CLI Reference](api-reference/cli.md) - All command line options
- [Configuration](api-reference/configuration.md) - Complete configuration reference
- [Template API](api-reference/templates.md) - Template system API

## 🎯 Key Features

### Modern Development Experience
- **Live Reload**: Instant browser updates with `sandra serve --watch`
- **File Watching**: Automatic rebuilds on file changes
- **Incremental Builds**: Only rebuild changed content for speed
- **Rich CLI**: Modern command-line interface with help and validation

### Enhanced Markdown
- **GitHub Flavored Markdown**: Tables, task lists, strikethrough
- **Emoji Support**: `:rocket:` → 🚀
- **Smart Typography**: Smart quotes, em-dashes, proper punctuation
- **Math Support**: KaTeX/MathJax integration (configurable)
- **Syntax Highlighting**: Client-side highlighting support

### Performance
- **5-10x Faster Builds**: Modern .NET 9 performance
- **Efficient Processing**: Markdig-based markdown processing
- **Smart Caching**: Template and content caching
- **Memory Optimized**: Efficient handling of large sites

### Cross-Platform
- **Windows, macOS, Linux**: Full cross-platform support
- **Global Tool**: Easy installation via `dotnet tool install`
- **Container Ready**: Works great in Docker containers
- **CI/CD Friendly**: Perfect for GitHub Actions, Azure Pipelines

## 🔄 Migration from v1

If you're migrating from Sandra.Snow v1:

```bash
# Automated migration
sandra migrate ./path/to/v1-site

# Review migration results
sandra build

# Start development
sandra serve --watch
```

See the [Migration Guide](migration/overview.md) for detailed instructions.

## 🤝 Contributing

Sandra.Snow v2 is open source and welcomes contributions:

- **Issues**: [Report bugs and feature requests](https://github.com/Sandra/Sandra.Snow/issues)
- **Discussions**: [Community discussions](https://github.com/Sandra/Sandra.Snow/discussions)  
- **Pull Requests**: [Contribute code improvements](https://github.com/Sandra/Sandra.Snow/pulls)
- **Documentation**: Help improve these docs

## 📋 Roadmap

### Current Status (v2.0.0-beta)
- ✅ Core static site generation
- ✅ Modern CLI interface
- ✅ Development server with live reload
- ✅ File watching and incremental builds
- ✅ Migration tools from v1

### Future Releases
- 🔄 **Enhanced Template System**: Full Razor support with TagHelpers
- 🔄 **Cloud Integration**: Deployment to major hosting platforms
- 🔄 **Plugin System**: Extensible architecture for custom features
- 🔄 **Theme Marketplace**: Easy theme sharing and distribution

## 🆘 Getting Help

### Common Issues
- [Troubleshooting Guide](troubleshooting.md) - Common problems and solutions
- [Performance Guide](performance.md) - Optimizing builds and development

### Support
- 📖 **Documentation**: This site
- 💬 **Community**: [GitHub Discussions](https://github.com/Sandra/Sandra.Snow/discussions)
- 🐛 **Bug Reports**: [GitHub Issues](https://github.com/Sandra/Sandra.Snow/issues)
- ✉️ **Email**: [support@sandra-snow.dev](mailto:support@sandra-snow.dev)

## 📄 License

Sandra.Snow v2 is released under the [MIT License](https://github.com/Sandra/Sandra.Snow/blob/main/LICENSE).

---

**Ready to start building amazing static sites with Sandra.Snow v2?** 

Jump to the [Quick Start Guide](tutorials/first-site.md) or migrate your existing site with the [Migration Guide](migration/overview.md)! 🚀
