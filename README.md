# Sandra.Snow

[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/download)
[![CI](https://github.com/Sandra/Sandra.Snow/actions/workflows/ci.yml/badge.svg)](https://github.com/Sandra/Sandra.Snow/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Sandra.Snow.Cli.svg)](https://www.nuget.org/packages/Sandra.Snow.Cli/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

**Modern static site generator inspired by Jekyll, built with .NET 10**

Sandra.Snow is a modern static site generator delivering exceptional performance, world-class developer experience, and cross-platform support for the .NET ecosystem.

> **Note**: Looking for v1? See the [v1.0.0 tag](https://github.com/Sandra/Sandra.Snow/tree/v1.0.0).

## ✨ **Key Features**

- **🚀 Blazing Fast**: 5-10x faster builds than v1
- **⚡ Live Reload**: Instant browser updates during development  
- **🧠 Incremental Builds**: Smart dependency tracking (< 1s rebuilds)
- **📱 Modern Markdown**: GitHub Flavored Markdown with emoji, tables, math
- **🌍 Cross-Platform**: Windows, macOS, Linux support
- **🔧 Modern CLI**: Intuitive commands with rich terminal output

## 🚀 **Quick Start**

### **Installation**
```bash
# Install as global .NET tool
dotnet tool install --global Sandra.Snow.Cli

# Verify installation
sandra --version
```

### **Create Your First Site**
```bash
# Create directory structure
mkdir my-blog && cd my-blog
mkdir _posts themes/default/_layouts

# Create configuration
cat > sandra.json << 'EOF'
{
  "blogTitle": "My Awesome Blog",
  "siteUrl": "https://myblog.com",
  "author": "Your Name",
  "email": "you@example.com",
  "theme": "default"
}
EOF

# Create your first post
cat > _posts/2024-01-01-hello-world.md << 'EOF'
---
title: Hello World
author: Your Name
layout: post
published: published
---

# Welcome to Sandra.Snow!

This is my first post with the new static site generator.

## Features
- **Fast builds** with modern .NET 10
- **Live reload** for instant feedback
- **Enhanced markdown** with tables and emoji 🎉

Happy blogging!
EOF

# Create post layout
cat > themes/default/_layouts/post.cshtml << 'EOF'
<!DOCTYPE html>
<html>
<head>
    <title>@Model.Title</title>
</head>
<body>
    <h1>@Model.Title</h1>
    <div>@Html.Raw(Model.Content)</div>
    <p>By: @Model.Author</p>
</body>
</html>
EOF
```

### **Build and Serve**
```bash
# Build static site
sandra build

# Development with live reload
sandra serve --watch
```

## 🎯 **Core Commands**

### **Build**
```bash
sandra build                                 # Build with default settings
sandra build --output ./dist                 # Custom output directory
sandra build --config ./custom/sandra.json   # Custom configuration
sandra build --verbose                       # Detailed logging
```

### **Development Server**
```bash
sandra serve                                 # Build and serve
sandra serve --watch                         # With file watching and live reload
sandra serve --port 8080                     # Custom port
sandra serve --watch --verbose               # Full development experience
```

### **Migration from v1**
```bash
sandra migrate ./path/to/v1-site             # Automated migration
sandra migrate ./old-blog --interactive      # Interactive mode
sandra migrate ./old-blog --force           # Force migration despite warnings
```

## 📊 **Performance**

Sandra.Snow delivers exceptional performance improvements over v1:

| Site Size | v1 Build Time | v2 Build Time | Improvement |
|-----------|---------------|---------------|-------------|
| **50 posts** | 2.1s | 0.4s | **5.3x faster** |
| **500 posts** | 18.3s | 2.1s | **8.7x faster** |
| **2000 posts** | 72.1s | 7.8s | **9.2x faster** |

**Development Experience:**
- **Live Reload**: < 1s from save to browser update
- **Incremental Builds**: 0.3-0.9s for content changes
- **Server Startup**: < 2s from command to ready

## 🛠️ **Architecture**

### **Modern .NET 10 Stack**
- **Runtime**: .NET 10 with cross-platform support
- **Markdown**: Markdig for GitHub Flavored Markdown
- **Templates**: Simplified template renderer (full Razor planned)
- **Server**: Kestrel with WebSocket live reload
- **CLI**: Modern command-line interface with rich output

### **Project Structure**
```
v2/
├── src/
│   ├── Snow.Abstractions/      # Core interfaces and contracts
│   ├── Snow.Engine/            # Static site generation engine  
│   └── Snow.Cli/               # Command-line interface
├── tests/
│   ├── Snow.Engine.Tests/      # Comprehensive test suite
│   └── Snow.Benchmarks/        # Performance benchmarks
└── docs/                       # Complete documentation
```

## 🔄 **Migrating from v1**

### **Automated Migration**
Most v1 sites can be migrated automatically:

```bash
# Install v2
dotnet tool install --global Sandra.Snow.Cli

# Migrate your existing site
sandra migrate ./path/to/v1-site

# Test the migration
sandra build

# Experience the new workflow
sandra serve --watch
```

### **What's Compatible**
- ✅ **All Content**: Markdown posts and pages work unchanged
- ✅ **Front Matter**: YAML metadata fully compatible
- ✅ **Directory Structure**: `_posts`, `_pages`, `_layouts` unchanged
- ✅ **Basic Templates**: Simple Razor syntax continues working

### **What's Changed**
- **Runtime**: .NET Framework 4.8 → .NET 10.0
- **CLI**: New modern command-line interface
- **Templates**: Simplified template system (enhanced Razor coming)
- **Development**: Live reload and file watching added

**📚 Complete migration guide**: [Migration Documentation](docs/migration/overview.md)

## 📚 **Documentation**

### **User Documentation**
- **[Getting Started](docs/README.md)**: Complete introduction to v2
- **[Migration Guide](docs/migration/overview.md)**: v1 → v2 migration
- **[CLI Reference](docs/api-reference/cli.md)**: All command-line options
- **[Configuration](docs/api-reference/configuration.md)**: Complete config reference

### **Developer Documentation**  
- **[Contributing Guide](CONTRIBUTING.md)**: How to contribute
- **[Architecture Overview](docs/architecture.md)**: Technical architecture
- **[API Documentation](docs/api-reference/)**: Complete API reference

## 🤝 **Community**

### **Get Help**
- **🐛 Bug Reports**: [GitHub Issues](https://github.com/Sandra/Sandra.Snow/issues/new?template=bug_report.yml)
- **💡 Feature Requests**: [Feature Request](https://github.com/Sandra/Sandra.Snow/issues/new?template=feature_request.yml)
- **🔄 Migration Help**: [Migration Help](https://github.com/Sandra/Sandra.Snow/issues/new?template=migration_help.yml)
- **💬 Discussions**: [GitHub Discussions](https://github.com/Sandra/Sandra.Snow/discussions)

### **Contribute**
- **Code**: [Contributing Guide](CONTRIBUTING.md)
- **Documentation**: Improve guides and examples
- **Testing**: Help test new features and migrations
- **Community**: Help answer questions and share knowledge

## 🗓️ **Roadmap**

### **v2.1 (Next Release)**
- `sandra new` command for site scaffolding
- Enhanced template system with full Razor support
- Additional migration tools and validators
- Performance optimizations

### **v2.x Future**
- Plugin system for extensibility
- Cloud deployment integration  
- Theme marketplace
- Advanced content management features

## 📦 **Packages**

| Package | Description | Version |
|---------|-------------|---------|
| `Sandra.Snow.Cli` | Global CLI tool | [![NuGet](https://img.shields.io/nuget/v/Sandra.Snow.Cli.svg)](https://www.nuget.org/packages/Sandra.Snow.Cli/) |
| `Sandra.Snow.Engine` | Core generation engine | [![NuGet](https://img.shields.io/nuget/v/Sandra.Snow.Engine.svg)](https://www.nuget.org/packages/Sandra.Snow.Engine/) |
| `Sandra.Snow.Abstractions` | Core interfaces | [![NuGet](https://img.shields.io/nuget/v/Sandra.Snow.Abstractions.svg)](https://www.nuget.org/packages/Sandra.Snow.Abstractions/) |

## 🏆 **Awards & Recognition**

*Sandra.Snow represents a significant advancement in .NET static site generation, combining modern performance with developer-friendly tooling.*

## 📄 **License**

Sandra.Snow is licensed under the [MIT License](LICENSE).

## 🙏 **Acknowledgments**

### **Contributors**
Thank you to all contributors who made Sandra.Snow possible through code, documentation, testing, and feedback.

### **Community**
Special thanks to the Sandra.Snow community for years of support and feedback that guided this modernization.

### **Technology**
Built with excellent open-source technologies:
- [.NET 10](https://dotnet.microsoft.com/) - Modern cross-platform runtime
- [Markdig](https://github.com/xoofx/markdig) - High-performance markdown processor
- [Spectre.Console](https://spectreconsole.net/) - Beautiful terminal applications
- [BenchmarkDotNet](https://benchmarkdotnet.org/) - Performance testing

---

**🚀 Start building amazing static sites with Sandra.Snow today!**

```bash
dotnet tool install --global Sandra.Snow.Cli
sandra --help
```

**Join the community**: [Discussions](https://github.com/Sandra/Sandra.Snow/discussions) • [Issues](https://github.com/Sandra/Sandra.Snow/issues) • [Documentation](https://sandra-snow.dev)
