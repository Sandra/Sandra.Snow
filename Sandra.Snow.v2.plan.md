# Sandra.Snow v2 Migration Plan - .NET 9.0 Modernization

## Executive Summary

This document outlines a comprehensive migration strategy for Sandra.Snow from .NET Framework 4.8 + NancyFX to .NET 9.0 + ASP.NET Core. The migration follows a staged approach with testable milestones, maintaining backward compatibility while modernizing the entire technology stack.

## Migration Objectives

### Primary Goals
- **Modernize Technology Stack**: Move to .NET 9.0, ASP.NET Core, and modern tooling
- **Maintain Functionality**: Preserve all existing features and capabilities
- **Ensure Backward Compatibility**: Support existing content, themes, and configurations
- **Improve Performance**: Leverage modern .NET performance improvements
- **Cross-Platform Support**: Enable running on Windows, Linux, and macOS
- **Enhanced Developer Experience**: Modern tooling, dependency injection, better testing

### Technology Stack Migration

| Component | Current (.NET 4.8) | Target (.NET 9.0) |
|-----------|-------------------|-------------------|
| Runtime | .NET Framework 4.8 | .NET 9.0 |
| Web Framework | NancyFX 1.4.5 | ASP.NET Core 9.0 |
| Hosting | OWIN/Nowin | Kestrel |
| Templates | Nancy.ViewEngines.Razor | ASP.NET Core Razor/RazorLight |
| Markdown | MarkdownSharp 1.13 | Markdig (latest) |
| JSON | Newtonsoft.Json 9.x | System.Text.Json + Newtonsoft (compat) |
| Testing | xUnit 2.x | xUnit 3.x |
| CLI | Custom args parsing | System.CommandLine |
| DI Container | None | Microsoft.Extensions.DependencyInjection |
| Configuration | Custom JSON | Microsoft.Extensions.Configuration |
| Logging | Console.WriteLine | Microsoft.Extensions.Logging + Serilog |

## Project Structure - /v2 Directory

```
Sandra.Snow/
├── v2/                              # New .NET 9 implementation
│   ├── src/
│   │   ├── Snow.Engine/            # Core generation engine (netstandard2.1)
│   │   ├── Snow.Cli/               # Command-line interface (net9.0)
│   │   ├── Snow.Web/               # Development web server (net9.0)
│   │   ├── Barbato.Web/            # CaaS web application (net9.0)
│   │   └── Snow.Abstractions/      # Shared interfaces (netstandard2.1)
│   ├── tests/
│   │   ├── Snow.Engine.Tests/
│   │   ├── Snow.Cli.Tests/
│   │   ├── Snow.Web.Tests/
│   │   └── Barbato.Web.Tests/
│   ├── samples/
│   │   └── SampleSite/             # Test site for validation
│   └── docs/
│       ├── migration-guide.md
│       └── api-documentation.md
└── (existing v1 code remains untouched)
```

## Migration Stages

### Stage 0: Pre-Migration Setup (1 week)

**Objective**: Establish foundation for migration with baseline testing and tooling.

#### Tasks
- [ ] Create `/v2` directory structure
- [ ] Set up .NET 9 solution file (`Sandra.Snow.v2.sln`)
- [ ] Configure GitHub Actions for CI/CD (.NET 9 matrix: Windows/Linux/macOS)
- [ ] Create golden-master test with sample site output hash validation
- [ ] Document current v1 behavior and output for regression testing
- [ ] Set up dependency analysis and security scanning

#### Acceptance Criteria
- CI pipeline runs successfully for .NET 9 projects
- Golden-master test captures current v1 output for comparison
- All tooling (formatters, analyzers, security scanners) configured
- Migration branch strategy established

### Stage 1: Core Engine Extraction (2 weeks)

**Objective**: Extract Nancy-independent core engine into separate project.

#### Tasks
- [ ] Create `Snow.Abstractions` project with interfaces:
  - [ ] `IMarkdownParser` interface
  - [ ] `ITemplateRenderer` interface  
  - [ ] `IFileProvider` abstractions for testing
  - [ ] `IContentProcessor` interface
- [ ] Create `Snow.Engine` project (netstandard2.1):
  - [ ] Move and refactor `SnowSettings` (remove Nancy dependencies)
  - [ ] Move `Models/` (Post, Page, Series, Category)
  - [ ] Move `PostParser`, `PagesParser` with interface implementation
  - [ ] Move `StaticFileProcessors/` with new base class
  - [ ] Move utility extensions (slug, date, IO helpers)
- [ ] Create adapter pattern for Nancy compatibility:
  - [ ] `NancyTemplateRenderer` wrapper for existing Nancy Razor engine
  - [ ] Keep existing v1 tests passing with adapter
- [ ] Comprehensive unit tests for engine components:
  - [ ] Post parsing with various front-matter combinations
  - [ ] URL generation with different formats
  - [ ] Series navigation and linking
  - [ ] Category grouping and filtering

#### Acceptance Criteria
- Engine compiles and runs on both .NET Framework 4.8 and .NET 9
- All existing functionality preserved via adapter pattern
- 90%+ code coverage on engine core components
- Zero Nancy dependencies in engine (except adapter)
- Golden-master test still passes

### Stage 2: Markdown Processing Modernization (3 days)

**Objective**: Replace MarkdownSharp with modern Markdig parser.

#### Tasks
- [ ] Implement `MarkdigParser` with `IMarkdownParser` interface
- [ ] Configure Markdig extensions for feature parity:
  - [ ] GitHub Flavored Markdown
  - [ ] Tables, task lists, strikethrough
  - [ ] Auto-links and emoji support
  - [ ] Syntax highlighting preparation
- [ ] Create markdown processing tests:
  - [ ] Standard markdown features
  - [ ] GFM extensions
  - [ ] Custom excerpt handling
  - [ ] Performance benchmarks vs MarkdownSharp
- [ ] Update configuration to support markdown options

#### Acceptance Criteria
- Markdig produces identical or improved HTML output
- All markdown tests pass with new parser
- Performance improvement documented (target: 25%+ faster)
- Configurable markdown extensions via settings

### Stage 3: Template System Modernization (2 weeks)

**Objective**: Replace Nancy Razor with ASP.NET Core Razor/RazorLight.

#### Tasks
- [ ] Implement `RazorLightTemplateRenderer`:
  - [ ] Template compilation and caching
  - [ ] Support for layout inheritance
  - [ ] Model binding for ViewModels
  - [ ] Theme path resolution
- [ ] Create template processing tests:
  - [ ] Layout rendering with models
  - [ ] Partial view support
  - [ ] Theme asset path resolution
  - [ ] Error handling for invalid templates
- [ ] Update ViewModels for .NET 9 compatibility:
  - [ ] System.Text.Json serialization attributes
  - [ ] Nullable reference type annotations
  - [ ] Record types where appropriate
- [ ] Performance optimization:
  - [ ] Template pre-compilation
  - [ ] Memory usage profiling
  - [ ] Async template rendering

#### Acceptance Criteria
- Template rendering produces pixel-perfect HTML output
- All existing ViewModels work with new renderer
- Template compilation errors provide clear diagnostics
- Performance matches or exceeds Nancy implementation

### Stage 4: Static Site Generation Pipeline (1 week)

**Objective**: Complete the core generation pipeline with modern dependencies.

#### Tasks
- [ ] Implement modern file processing:
  - [ ] `Microsoft.Extensions.FileProviders` integration
  - [ ] Async file operations throughout
  - [ ] Memory-efficient large file handling
- [ ] Update processor architecture:
  - [ ] Dependency injection for processors
  - [ ] Async processing support
  - [ ] Progress reporting and cancellation
- [ ] Enhanced error handling:
  - [ ] Structured logging with `ILogger`
  - [ ] Detailed error context and suggestions
  - [ ] Graceful degradation for missing files
- [ ] Configuration validation:
  - [ ] JSON schema validation
  - [ ] Required field validation
  - [ ] Path resolution validation

#### Acceptance Criteria
- Complete static site generation without Nancy dependencies
- All processors work with dependency injection
- Comprehensive error messages guide users to solutions
- Configuration validation prevents common setup errors

### Stage 5: Command-Line Interface (1 week)

**Objective**: Modern CLI with System.CommandLine and global tool support.

#### Tasks
- [ ] Create `Snow.Cli` project:
  - [ ] `System.CommandLine` command structure
  - [ ] Preserve all existing command-line options
  - [ ] Enhanced help documentation
  - [ ] Configuration file discovery and validation
- [ ] Global tool packaging:
  - [ ] NuGet package configuration
  - [ ] Multi-platform binaries
  - [ ] Self-contained deployment options
- [ ] CLI testing:
  - [ ] End-to-end command execution tests
  - [ ] Configuration file scenarios
  - [ ] Error condition handling
  - [ ] Performance regression tests
- [ ] Enhanced user experience:
  - [ ] Progress indicators for long operations
  - [ ] Colored console output
  - [ ] Verbose/quiet modes

#### Acceptance Criteria
- CLI maintains 100% compatibility with v1 command structure
- Global tool installs and runs on Windows, Linux, macOS
- Clear error messages and help documentation
- Performance matches or exceeds v1 CLI

### Stage 6: Development Web Server (3 days)

**Objective**: Replace OWIN with Kestrel-based ASP.NET Core server.

#### Tasks
- [ ] Create `Snow.Web` project:
  - [ ] ASP.NET Core minimal API setup
  - [ ] Static file serving with proper MIME types
  - [ ] Directory browsing support
  - [ ] Custom port configuration
- [ ] Live-reload capabilities:
  - [ ] File watcher for content changes
  - [ ] SignalR for browser refresh
  - [ ] Incremental rebuilds
- [ ] Development features:
  - [ ] Request logging and diagnostics
  - [ ] Error pages with source maps
  - [ ] Performance profiling endpoints
- [ ] Integration testing:
  - [ ] Web server startup and shutdown
  - [ ] Static file serving accuracy
  - [ ] Live-reload functionality

#### Acceptance Criteria
- Development server serves identical content to v1
- Live-reload works for content and theme changes
- Server starts faster than v1 OWIN implementation
- All static file types serve correctly

### Stage 7: Advanced Features & Optimization (1 week)

**Objective**: Implement advanced features and performance optimizations.

#### Tasks
- [ ] Series functionality enhancement:
  - [ ] Improved series navigation UI
  - [ ] Series-level metadata and templates
  - [ ] Cross-series references
- [ ] Category system improvements:
  - [ ] Hierarchical categories
  - [ ] Category templates and metadata
  - [ ] Tag system integration
- [ ] Performance optimizations:
  - [ ] Incremental builds (file change detection)
  - [ ] Parallel processing where safe
  - [ ] Memory usage optimization
  - [ ] Build time profiling and reporting
- [ ] Enhanced theme system:
  - [ ] Theme package management
  - [ ] Multiple theme support
  - [ ] Theme inheritance and overrides

#### Acceptance Criteria
- All advanced features work as in v1 or better
- Build performance improved by 30%+ for large sites
- Memory usage reduced for large content sets
- Theme system provides better developer experience

### Stage 8: Barbato Web Application Rewrite (4 weeks)

**Objective**: Complete rewrite of Barbato CaaS using ASP.NET Core.

#### Tasks
- [ ] Project setup and architecture:
  - [ ] ASP.NET Core MVC + Razor Pages structure
  - [ ] Modern authentication with ASP.NET Core Identity
  - [ ] OAuth provider integration (GitHub, etc.)
  - [ ] Entity Framework Core for data storage
- [ ] API endpoint migration:
  - [ ] Repository management endpoints
  - [ ] Build trigger and status APIs
  - [ ] Deployment configuration APIs
  - [ ] Webhook receivers (GitHub, GitLab, etc.)
- [ ] User interface modernization:
  - [ ] Responsive Bootstrap 5 design
  - [ ] Real-time build status updates
  - [ ] Modern JavaScript (ES6+, no jQuery dependency)
  - [ ] Progressive Web App features
- [ ] Enhanced features:
  - [ ] Build history and logs
  - [ ] Multi-site management
  - [ ] Deployment target management (FTP, Azure, AWS)
  - [ ] Environment-specific configurations

#### Acceptance Criteria
- All Barbato v1 functionality preserved and enhanced
- Modern, responsive user interface
- Improved security with ASP.NET Core Identity
- Better performance and reliability

### Stage 9: Migration Tools & Documentation (1 week)

**Objective**: Provide tools and documentation for smooth migration.

#### Tasks
- [ ] Migration tools:
  - [ ] Configuration file migration utility
  - [ ] Theme compatibility checker
  - [ ] Content validation tool
  - [ ] Performance comparison utility
- [ ] Comprehensive documentation:
  - [ ] Migration guide from v1 to v2
  - [ ] Breaking changes documentation
  - [ ] New features and improvements guide
  - [ ] Theme development guide for v2
- [ ] Examples and samples:
  - [ ] Updated sample sites
  - [ ] Custom processor examples
  - [ ] Theme development examples
  - [ ] Deployment configuration examples

#### Acceptance Criteria
- Migration tools handle 95% of common scenarios automatically
- Documentation covers all migration scenarios
- Examples provide clear guidance for customization
- Community can migrate existing sites without manual intervention

### Stage 10: Release Preparation & Launch (1 week)

**Objective**: Prepare for v2.0.0 release with full testing and documentation.

#### Tasks
- [ ] Release preparation:
  - [ ] Final integration testing across all platforms
  - [ ] Performance benchmarking vs v1
  - [ ] Security audit and vulnerability scanning
  - [ ] Accessibility testing for Barbato web UI
- [ ] Package preparation:
  - [ ] NuGet packages for all components
  - [ ] Global tool publishing
  - [ ] Docker images for Barbato
  - [ ] Chocolatey/Homebrew packages
- [ ] Launch activities:
  - [ ] Release notes with migration guide
  - [ ] Community announcement
  - [ ] Blog posts and documentation updates
  - [ ] GitHub release with binaries

#### Acceptance Criteria
- All packages published and installable
- Release documentation complete and accurate
- Community has clear path to adopt v2
- Long-term support plan for v1 established

## Testing Strategy

### Unit Testing Requirements
- **Minimum Coverage**: 90% code coverage across all projects
- **Test Frameworks**: xUnit 3.x with FluentAssertions
- **Mock Framework**: NSubstitute or Moq for interface mocking
- **Test Data**: Approval tests for golden master validation

### Integration Testing
- **File System Testing**: Use `System.IO.Abstractions` for testable file operations
- **Web Testing**: `WebApplicationFactory<T>` for Barbato integration tests
- **CLI Testing**: Process execution tests in isolated directories
- **Performance Testing**: Benchmark tests comparing v1 and v2 performance

### Testing Infrastructure
- **CI/CD**: GitHub Actions with matrix builds (Windows, Linux, macOS)
- **Test Data**: Shared test content and expected output validation
- **Performance Monitoring**: Automated performance regression detection
- **Security Testing**: Static analysis and dependency vulnerability scanning

## Risk Management

### Technical Risks
1. **Template Compatibility**: RazorLight may not support all Nancy Razor features
   - *Mitigation*: Comprehensive template testing and fallback options
2. **Performance Regression**: .NET 9 changes might impact build performance
   - *Mitigation*: Continuous performance monitoring and optimization
3. **Breaking Changes**: System.Text.Json serialization differences
   - *Mitigation*: Maintain Newtonsoft.Json compatibility layer

### Migration Risks
1. **Content Incompatibility**: Existing sites may not work with v2
   - *Mitigation*: Extensive backward compatibility testing and migration tools
2. **Theme Breaking Changes**: Themes may need updates for v2
   - *Mitigation*: Theme compatibility checker and migration guide
3. **Configuration Changes**: Settings format changes could break automation
   - *Mitigation*: Configuration migration utility and validation

## Success Metrics

### Technical Metrics
- **Performance**: 25%+ faster build times for typical sites
- **Memory Usage**: 20%+ reduction in memory consumption
- **Startup Time**: 50%+ faster CLI and web server startup
- **Test Coverage**: 90%+ code coverage across all projects

### Quality Metrics
- **Backward Compatibility**: 95%+ of existing sites work without modification
- **Bug Reports**: <5 critical bugs in first 30 days post-release
- **Documentation**: Complete migration documentation with examples
- **Community Adoption**: Clear migration path with tooling support

## Timeline & Resource Requirements

### Overall Timeline: 12-14 weeks
- **Pre-Migration**: 1 week
- **Core Engine**: 3 weeks (Stages 1-4)
- **Modern CLI/Web**: 2 weeks (Stages 5-6)
- **Advanced Features**: 1 week (Stage 7)
- **Barbato Rewrite**: 4 weeks (Stage 8)
- **Migration Tools**: 1 week (Stage 9)
- **Release Prep**: 1 week (Stage 10)

### Resource Requirements
- **Senior .NET Developer**: Full-time for architecture and complex components
- **Mid-Level Developer**: Full-time for implementation and testing
- **Part-time Support**: DevOps engineer for CI/CD, documentation writer for guides

## Conclusion

This migration plan provides a structured approach to modernizing Sandra.Snow while maintaining its core value proposition. The staged approach ensures each milestone delivers working, testable functionality, reducing risk and enabling early feedback. The comprehensive testing strategy ensures reliability, while the focus on backward compatibility minimizes disruption for existing users.

The migration to .NET 9.0 and ASP.NET Core positions Sandra.Snow for long-term maintainability, better performance, and enhanced features while preserving the familiar developer experience that makes it valuable to the .NET community.
