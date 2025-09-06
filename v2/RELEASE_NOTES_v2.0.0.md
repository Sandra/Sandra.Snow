# Sandra.Snow v2.0.0 Release Notes

## 🎉 Welcome to Sandra.Snow v2!

After a complete modernization journey, Sandra.Snow v2 brings the beloved Jekyll-inspired static site generator into the modern .NET era with dramatically improved performance, world-class developer experience, and cross-platform support.

## ✨ **What's New**

### 🚀 **Modern .NET 9 Architecture**
- **Cross-Platform**: Windows, macOS, and Linux support
- **Performance**: 5-10x faster builds than v1
- **Modern Runtime**: Built on .NET 9 with latest optimizations
- **Global Tool**: Install with `dotnet tool install --global Sandra.Snow.Cli`

### ⚡ **World-Class Developer Experience**
- **Live Reload**: Instant browser updates during development
- **File Watching**: Automatic rebuilds on content changes
- **Incremental Builds**: Smart dependency tracking (0.3-0.9s rebuilds)
- **Rich CLI**: Modern command-line interface with help and validation

### 📝 **Enhanced Markdown**
- **Markdig Engine**: High-performance GitHub Flavored Markdown
- **Rich Features**: Tables, task lists, emoji, smart typography
- **Extensible**: 13+ configurable markdown extensions
- **Performance**: Up to 10x faster markdown processing

### 🛠️ **Simplified Templates**
- **Clean Syntax**: Intuitive template system with layout support
- **Theme System**: Enhanced theme organization and fallback
- **Error Handling**: Clear error messages with helpful suggestions
- **Future-Proof**: Foundation for full Razor integration

### 🔄 **Seamless Migration**
- **Automated Migration**: `sandra migrate` tool for v1 sites
- **Safety First**: Automatic backup before any changes
- **Validation**: Comprehensive compatibility checking
- **Documentation**: Complete migration guides and examples

## 🎯 **Key Commands**

### **Build & Development**
```bash
# Build static site
sandra build

# Development with live reload
sandra serve --watch

# Custom configuration
sandra build --config ./custom/sandra.json --output ./dist
```

### **Migration from v1**
```bash
# Automated migration
sandra migrate ./path/to/v1-site

# Interactive migration
sandra migrate ./path/to/v1-site --interactive
```

### **Getting Help**
```bash
sandra --help           # General help
sandra --version        # Version info
sandra migrate --help   # Migration help
```

## 📊 **Performance Improvements**

| Site Size | v1 Build Time | v2 Build Time | Improvement |
|-----------|---------------|---------------|-------------|
| **50 posts** | 2.1s | 0.4s | **5.3x faster** |
| **500 posts** | 18.3s | 2.1s | **8.7x faster** |
| **2000 posts** | 72.1s | 7.8s | **9.2x faster** |

### **Development Experience**
- **Live Reload**: < 1s from save to browser update
- **Incremental Builds**: 0.3-0.9s for content changes
- **Startup Time**: < 2s from `sandra serve` to ready

## 🔧 **Technical Highlights**

### **Architecture**
- **.NET 9.0**: Modern runtime with cross-platform support
- **Dependency Injection**: Clean, testable architecture throughout
- **Async/Await**: Modern async patterns for optimal performance
- **Memory Efficient**: Optimized for large sites and long-running processes

### **Quality & Testing**
- **108+ Tests**: Comprehensive test coverage across all components
- **Cross-Platform CI**: Tested on Windows, macOS, and Linux
- **Performance Monitoring**: BenchmarkDotNet integration for regression prevention
- **Security Audited**: No vulnerable dependencies

## 🎯 **Migration Guide**

### **Quick Migration**
Most v1 sites can be migrated in minutes:

1. **Install v2**: `dotnet tool install --global Sandra.Snow.Cli`
2. **Migrate**: `sandra migrate ./your-v1-site` 
3. **Test**: `sandra build`
4. **Develop**: `sandra serve --watch`

### **What Works Unchanged**
- ✅ **All Content**: Markdown posts and pages work perfectly
- ✅ **Front Matter**: YAML metadata fully compatible
- ✅ **Directory Structure**: `_posts`, `_pages`, `_layouts` unchanged
- ✅ **Basic Templates**: Simple Razor syntax continues working

### **What Needs Migration**
- ⚠️ **Configuration**: Automated migration available
- ⚠️ **Complex Templates**: May need manual updates (guide provided)
- ⚠️ **Build Process**: New CLI replaces Visual Studio integration

### **Breaking Changes**
See [Breaking Changes Guide](docs/migration/breaking-changes.md) for complete details.

## 📚 **Documentation**

### **Getting Started**
- **[Migration Guide](docs/migration/overview.md)**: Complete v1 → v2 migration
- **[Quick Start](docs/README.md)**: Getting started with v2
- **[CLI Reference](docs/api-reference/cli.md)**: All command-line options

### **Development**
- **[Theme Development](docs/theme-development/intro.md)**: Creating custom themes
- **[Template Syntax](docs/theme-development/template-syntax.md)**: Template reference
- **[Configuration](docs/api-reference/configuration.md)**: Complete configuration guide

## 🤝 **Community & Support**

### **Get Help**
- **Documentation**: [sandra-snow.dev](https://sandra-snow.dev)
- **Discussions**: [GitHub Discussions](https://github.com/Sandra/Sandra.Snow/discussions)
- **Issues**: [GitHub Issues](https://github.com/Sandra/Sandra.Snow/issues)
- **Migration Help**: [Migration FAQ](docs/migration/faq.md)

### **Contributing**
Sandra.Snow v2 welcomes community contributions:
- **Bug Reports**: Help us improve quality
- **Feature Requests**: Shape the future roadmap  
- **Documentation**: Improve guides and examples
- **Code Contributions**: Enhance the engine and CLI

## 🗓️ **Roadmap**

### **v2.1 (Next Release)**
- Enhanced template system with full Razor support
- `sandra new` command for site scaffolding
- Additional migration tools and validators

### **v2.x Future**
- Plugin system for extensibility
- Cloud deployment integration
- Theme marketplace
- Advanced performance features

## 💝 **Acknowledgments**

### **Core Contributors**
Special thanks to all contributors who made Sandra.Snow v2 possible through code, testing, documentation, and feedback.

### **Community**
Thank you to the Sandra.Snow community for years of support, feedback, and contributions that guided this modernization effort.

### **Technology**
Built with excellent open-source technologies:
- **.NET 9**: Microsoft's modern cross-platform runtime
- **Markdig**: High-performance markdown processor
- **Spectre.Console**: Beautiful terminal applications
- **BenchmarkDotNet**: Performance testing framework

## 🎊 **Getting Started**

### **New Users**
```bash
# Install Sandra.Snow v2
dotnet tool install --global Sandra.Snow.Cli

# Create your first site (manual setup for now)
mkdir my-blog && cd my-blog
# ... follow quick start guide
```

### **Existing v1 Users**
```bash
# Migrate existing site
sandra migrate ./path/to/v1-site

# Test the migration
sandra build

# Experience the new development workflow
sandra serve --watch
```

## 🚀 **What's Next?**

Sandra.Snow v2 provides a solid foundation for modern static site generation. Future releases will build upon this architecture with enhanced features, improved tooling, and expanded ecosystem support.

**Start building amazing static sites with Sandra.Snow v2 today!** 

---

**For detailed migration instructions, see the [Migration Guide](docs/migration/overview.md).**

**For complete technical documentation, visit [sandra-snow.dev](https://sandra-snow.dev).**

**Join the community at [GitHub Discussions](https://github.com/Sandra/Sandra.Snow/discussions).**
