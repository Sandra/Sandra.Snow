# Sandra.Snow v1 to v2 Migration Guide

## Overview

Sandra.Snow v2 represents a complete modernization of the static site generator, moving from .NET Framework 4.8 + NancyFX to .NET 9.0 with modern tooling and significantly improved performance. This guide helps you migrate your existing v1 sites to v2.

## Quick Start

The fastest way to migrate is using the automated migration tool:

```bash
# Install Sandra.Snow v2 CLI
dotnet tool install --global Sandra.Snow.Cli

# Migrate your v1 site
sandra migrate ./my-v1-blog

# Test the migration
sandra build

# Start development with live reload
sandra serve --watch
```

## Migration Overview

### What's Improved in v2

- **🚀 Performance**: 5-10x faster builds with Markdig + incremental builds
- **⚡ Development Experience**: Live reload, file watching, modern CLI
- **🔧 Modern Stack**: .NET 9, cross-platform, better error handling
- **📱 Better Markdown**: GitHub Flavored Markdown, tables, emoji, math support
- **🎯 Simplified Templates**: Cleaner template syntax, better error messages

### What's Changed

- **Runtime**: .NET Framework 4.8 → .NET 9.0
- **Templates**: Nancy Razor → Simple template renderer (enhanced Razor coming in future releases)
- **CLI**: Custom args → Modern CLI with subcommands
- **Configuration**: Minor changes to config format
- **Development**: OWIN → Kestrel with live reload

## Migration Process

### Step 1: Install Sandra.Snow v2

```bash
# Remove v1 (if installed globally)
# No uninstall needed for v1 - it's typically run from Visual Studio

# Install v2 CLI tool
dotnet tool install --global Sandra.Snow.Cli

# Verify installation
sandra --version
```

### Step 2: Prepare Your v1 Site

Before migration, ensure your v1 site follows these requirements:

**✅ Required:**
- `_posts` directory with markdown files
- `snow.config` or `snow.config.json` configuration file
- Post files follow `YYYY-MM-DD-slug.md` naming convention

**📋 Recommended:**
- `_pages` directory for static pages
- `_layouts` directory for templates
- `themes` directory for custom themes

### Step 3: Run Migration Tool

```bash
# Basic migration (recommended)
sandra migrate ./path/to/v1-site

# Interactive migration (for complex configurations)
sandra migrate ./path/to/v1-site --interactive

# Custom output location
sandra migrate ./path/to/v1-site --output ./new-site/sandra.json
```

### Step 4: Review Migration Results

The migration tool will:
- ✅ **Backup Original**: Creates `snow.config.v1-backup-[timestamp]`
- ✅ **Convert Configuration**: Maps v1 settings to v2 format
- ✅ **Validate Content**: Checks posts and pages for compatibility
- ✅ **Report Issues**: Lists warnings and required manual fixes
- ✅ **Generate New Config**: Creates `sandra.json` for v2

### Step 5: Test the Migration

```bash
# Build with new configuration
sandra build

# Start development server
sandra serve

# Development with live reload
sandra serve --watch
```

### Step 6: Address Warnings (If Any)

Common migration warnings and their solutions:

#### Template Warnings
```
⚠ Nancy syntax detected: post.cshtml
  Nancy Razor helpers (@Html.*, @Url.*) may need manual conversion
```

**Solution**: Update template syntax (see [Template Migration Guide](template-conversion.md))

#### Configuration Warnings
```
⚠ Removed obsolete setting: server
  Server functionality replaced with 'sandra serve' command
```

**Solution**: Use CLI commands instead of configuration flags

#### Content Warnings
```
⚠ File naming: my-post.md
  Posts should follow YYYY-MM-DD-slug.md convention
```

**Solution**: Rename files to follow v2 conventions

## Common Migration Scenarios

### Simple Blog Migration

**v1 Structure:**
```
my-blog/
├── _posts/
│   └── 2023-01-01-hello-world.md
├── themes/default/
│   └── _layouts/post.cshtml
└── snow.config.json
```

**Migration Steps:**
1. `sandra migrate ./my-blog`
2. `sandra build` (test)
3. `sandra serve --watch` (develop)

**Result**: Fully working v2 site with improved performance

### Complex Site Migration

**v1 Structure with Custom Features:**
```
company-site/
├── _posts/
├── _pages/
├── themes/custom/
├── assets/
└── snow.config (with custom settings)
```

**Migration Steps:**
1. `sandra migrate ./company-site --interactive`
2. Review warnings for custom template syntax
3. Manually update templates if needed
4. Test thoroughly: `sandra build`
5. Update deployment scripts to use new CLI

## Migration Checklist

### Pre-Migration
- [ ] Backup your entire v1 site
- [ ] Ensure v1 site builds successfully
- [ ] Document any custom modifications
- [ ] Note any custom OWIN/Nancy extensions used

### Migration
- [ ] Run `sandra migrate` tool
- [ ] Review all warnings and errors
- [ ] Test build with `sandra build`
- [ ] Verify generated HTML matches expectations
- [ ] Test development server: `sandra serve`

### Post-Migration
- [ ] Update deployment scripts/CI
- [ ] Update documentation to use v2 CLI
- [ ] Test live reload: `sandra serve --watch`
- [ ] Update team workflows for new CLI
- [ ] Consider performance optimizations

## Troubleshooting

### Migration Tool Errors

**"No v1 configuration file found"**
```bash
# Ensure you have snow.config or snow.config.json
ls -la snow.config*

# If missing, create a basic v1 config first
```

**"Validation failed"**
```bash
# Use --force to continue despite warnings
sandra migrate ./my-site --force

# Or fix validation issues first
sandra migrate ./my-site --help
```

### Build Errors After Migration

**"Template not found"**
- Check that `_layouts` directory exists
- Verify template files have `.cshtml` extension
- Ensure theme directory structure is correct

**"Posts not found"**
- Verify `_posts` directory path in configuration
- Check file naming convention: `YYYY-MM-DD-slug.md`
- Ensure files have proper YAML front matter

### Performance Issues

**"Builds are slower than expected"**
- Use `sandra serve --watch` for development (incremental builds)
- Check that large asset directories are in ignore patterns
- Consider file system performance on your platform

## Getting Help

### Documentation
- [Breaking Changes Guide](breaking-changes.md) - Complete list of changes
- [Template Migration](template-conversion.md) - Template syntax updates  
- [Configuration Reference](../api-reference/configuration.md) - Complete config options
- [CLI Reference](../api-reference/cli.md) - All command line options

### Support
- 🐛 **Issues**: [GitHub Issues](https://github.com/Sandra/Sandra.Snow/issues)
- 💬 **Discussions**: [GitHub Discussions](https://github.com/Sandra/Sandra.Snow/discussions)
- 📖 **Documentation**: [Sandra.Snow Docs](https://sandra.snow.dev)

### Migration Examples
- [Example Blog Migration](../examples/blog-migration/) - Complete example walkthrough
- [Theme Migration](../examples/theme-migration/) - Custom theme conversion
- [Complex Site Migration](../examples/complex-migration/) - Advanced scenarios

## Next Steps

After successful migration:

1. **Explore v2 Features**: Try live reload, enhanced markdown, better CLI
2. **Performance**: Measure the improvement with `sandra benchmark`
3. **Contribute**: Share your migration experience to help others
4. **Upgrade Themes**: Take advantage of v2's improved template system

**🎉 Welcome to Sandra.Snow v2!**

Your site now benefits from modern .NET performance, excellent development experience, and a solid foundation for future enhancements.
