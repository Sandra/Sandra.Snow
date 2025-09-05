# Sandra.Snow - Comprehensive Technical Documentation

## Overview

Sandra.Snow is a Jekyll-inspired static site generator built with C# and .NET Framework 4.8. It provides a powerful, flexible solution for generating static websites from Markdown content with sophisticated templating capabilities using Razor views. The system can run as a command-line tool, self-hosted web server, or as a "Compiler-as-a-Service" (CaaS) through the Barbato web application.

## Quick Start (5 Minutes)

1. **Clone and Build**:
   ```bash
   git clone https://github.com/Sandra/Sandra.Snow.git
   cd Sandra.Snow
   ```

2. **Open in Visual Studio**: Open `src/Sandra.Snow.sln`

3. **Run**: Press F5 to build and run with the included SnowSite sample

4. **View**: Browser opens automatically to `http://localhost:5498` showing the generated site

5. **Customize**: Edit files in `SnowSite/Snow/_posts/` and `SnowSite/Snow/snow.config.json`

The sample site demonstrates posts, pages, categories, series, and all core features.

## Key Features

- **Jekyll-inspired workflow**: Familiar directory structure and conventions (`_posts`, `_pages`, `_layouts`)
- **Razor templating**: Full-featured Razor view engine with strong typing support
- **Built-in web server**: OWIN-based self-hosting for local development and preview (port 5498)
- **Series support**: First-class multi-part article management with automatic linking
- **Draft system**: Compile but hide draft content with optional preview capabilities
- **Private posts**: Generated but excluded from output (Published.Private state)
- **Extensible processing**: Plugin architecture for custom content processors
- **Theme system**: Switchable themes with asset management
- **CaaS via Barbato**: Cloud-ready with GitHub webhook integration and automated deployment
- **Strong typing**: C#-based models with rich IntelliSense support in Razor templates

## Architecture & Solution Structure

### Project Layout
```
Sandra.Snow/
├── src/
│   ├── Sandra.Snow.sln         # Main solution file
│   ├── Snow/                   # Core static site generator
│   ├── Snow.Tests/             # Unit tests for Snow
│   ├── Barbato/               # Web-based CaaS application
│   └── Barbato.Tests/         # Unit tests for Barbato
├── SnowSite/                  # Sample/demo site
│   └── Snow/                  # Site content and configuration
├── Snow.CLI/                  # (Legacy directory structure)
├── Snow.Core/                 # (Legacy directory structure)
├── Snow.Core.Tests/          # (Legacy directory structure)
└── _sample-deployment/       # Deployment samples
```

### Core Components

#### Snow Project (Main Engine)
- **Program.cs**: Entry point and orchestration engine
- **SnowSettings.cs**: Configuration management and defaults
- **Models/**: Core data models (Post, Page, Series, Category)
- **PostParser.cs/PagesParser.cs**: Markdown parsing and front-matter processing
- **StaticFileProcessors/**: Extensible content processors
- **ViewModels/**: Strongly-typed data for Razor templates
- **Extensions/**: Utility helpers (slug generation, Gravatar, IO operations)
- **TestModule.cs**: Nancy module for in-memory view rendering
- **SnowServer.cs**: OWIN self-host web server

#### Barbato Project (CaaS Web Application)
- Web application for remote compilation triggered by GitHub webhooks
- User authentication and repository management
- Automated deployment capabilities (FTP, Azure)
- Built with NancyFX and Bootstrap frontend

## Content Processing Pipeline

### 1. Initialization & Configuration
```
Snow.exe config=<path> [server=true] [debug=true]
```
- Parse command-line arguments
- Load `snow.config.json` from specified directory
- Merge with default settings

### 2. Content Discovery & Parsing
- **Posts**: Scan `_posts/` for files matching `YYYY-MM-DD-slug.{md|markdown}`
- **Pages**: Scan `_pages/` for markdown files (no date requirement)
- Parse YAML-like front matter headers (simplified subset, not full YAML 1.2)
- Convert Markdown to HTML using MarkdownSharp
- Extract metadata (categories, series, published status, etc.)

### 3. Data Model Construction
- Create Post/Page objects with parsed content
- Build category collections and archives
- Generate series navigation
- Sort and filter based on published status

### 4. Template Rendering
- Initialize Nancy Browser with in-memory test module
- Configure Razor view engine (SuperSimple reference exists but may not be functional)
- Set up theme-based static path provider using SnowViewLocationConventions
- For each Post/Page:
  - Assign to TestModule.Data
  - POST to `/compose` endpoint
  - Render using appropriate layout via layout resolution
  - Write to output directory structure

### 5. Static File Processing
- Process `processFiles` configuration array
- Generate index pages, category listings, archives
- Create RSS/Atom feeds
- Generate sitemaps
- Each processor receives `SnowyData` context

### 6. Asset Management
- Copy theme assets (CSS, JavaScript, images)
- Copy configured directories and files
- Support for source → destination mapping

### 7. Server Mode (Optional)
- Start OWIN self-host on localhost:5498
- Serve generated static files using Microsoft.Owin.StaticFiles
- Live preview capabilities with directory browsing enabled

## Configuration (snow.config / snow.config.json)

Sandra.Snow looks first for `snow.config` (no extension), then falls back to `snow.config.json`.

### Core Settings
```json
{
  "blogTitle": "My Blog",
  "siteUrl": "https://example.com",
  "author": "Author Name",
  "email": "author@example.com"
}
```

### Directory Structure
```json
{
  "posts": "_posts",              // Source posts directory
  "pages": "_pages",              // Source pages directory  
  "layouts": "_layouts",          // Template layouts
  "theme": "default",             // Active theme name
  "postsOutput": "Website",       // Generated posts destination
  "pagesOutput": "Website"        // Generated pages destination
}
```

### URL Formats
```json
{
  "urlFormat": "yyyy/MM/dd/slug", // Post URL pattern (default)
  "pageUrlFormat": "slug"         // Page URL pattern
}
```
Available tokens: `yyyy` (year), `MM` (month), `dd` (day), `slug` (filename slug)

### Asset Management
```json
{
  "copyDirectories": [
    "images",
    "javascripts => js",          // Support for source => destination mapping
    "stylesheets"
  ],
  "copyFiles": [
    "favicon.ico",
    "robots.txt"
  ]
}
```

Asset copying supports mapping syntax: `"source => destination"` allows renaming directories during copy.

### Static File Processing

**Built-in Processors Available:**
- `Posts`: Main blog posts listing and pagination
- `Drafts`: Draft posts listing
- `Categories`: Category-based post listings
- `atom`: Atom feed generation
- `rss`: RSS feed generation
- `sitemap`: XML sitemap generation
```json
{
  "processFiles": [
    {"file": "index.cshtml", "loop": "Posts"},
    {"file": "category.cshtml", "loop": "Categories"},
    {"file": "archive.cshtml", "title": "Archive"},
    {"file": "feed.xml", "loop": "atom"}
  ]
}
```

### Layout Defaults
```json
{
  "defaultPostLayout": "post",
  "defaultPageLayout": "page"
}
```

### Pagination
```json
{
  "pageSize": 10,    // Items per page in listings
  "feedSize": 10     // Items in RSS/Atom feeds
}
```

The PostsProcessor automatically implements pagination, exposing paged post collections in TestModule controlled by the PageSize setting.

## Content Creation

### Post Structure
Posts must follow the naming convention: `YYYY-MM-DD-slug.{md|markdown}`

```markdown
---
published: published
layout: post
category: Programming
title: My Blog Post Title
author: Custom Author
series:
    name: "My Series"
    current: 1
    part: First Part
    part: Second Part
tags: programming, csharp, web
---

This is the post content in Markdown.

<!--excerpt-->

This creates an excerpt break for listing pages.
```

**Published States**: 
- `published` (or `true`): Included in output and listings
- `draft`: Generated but hidden from main listings (available in drafts page)
- `private`: Generated but excluded from all output

### Page Structure
Pages have no naming restrictions and support the same front matter:

```markdown
---
published: published
layout: page
title: About Me
---

Page content here.
```

### Excerpt Handling
Two methods for excerpts:

1. **Inline excerpt marker**:
   ```markdown
   Content before excerpt.
   <!--excerpt-->
   Content after excerpt (not shown in listings).
   ```

2. **Dedicated excerpt section**:
   ```markdown
   ---
   published: published
   title: Post Title
   ---
   
   ---excerpt
   This is the dedicated excerpt content.
   ---end
   
   This is the main post content.
   ```

### Series Support
Multi-part articles with automatic navigation:

```markdown
---
series:
    name: "Learning C#"
    current: 2
    part: Introduction
    part: Variables and Types
    part: Control Flow
---
```

## Templating System

### Razor Templates
Full Razor syntax support with strongly-typed models:

```html
@model Snow.ViewModels.PostViewModel

<article>
    <h1>@Model.Title</h1>
    <time datetime="@Model.Date.ToString("yyyy-MM-dd")">
        @Model.Date.ToString("MMMM dd, yyyy")
    </time>
    <div class="content">
        @Html.Raw(Model.Content)
    </div>
    
    @if(Model.Series != null)
    {
        <nav class="series">
            <!-- Series navigation -->
        </nav>
    }
</article>
```

### Available View Models
- **PostViewModel**: Individual post data
- **ContentViewModel**: Page data  
- **BaseViewModel**: Common site data
- Collections: Posts, Drafts, Categories, Archives

### Theme Structure
```
themes/
└── mytheme/
    ├── _layouts/
    │   ├── default.cshtml      # Master layout
    │   ├── post.cshtml         # Post template
    │   └── page.cshtml         # Page template
    ├── stylesheets/
    ├── javascripts/
    ├── images/
    ├── index.cshtml            # Home page
    ├── category.cshtml         # Category listing
    ├── archive.cshtml          # Archive page
    └── feed.xml               # RSS/Atom template
```

## Extensibility

### Custom Processors
Derive from `BaseProcessor` to handle custom content types:

```csharp
public class CustomProcessor : BaseProcessor
{
    public override string ProcessorName => "custom";

    protected override void Impl(SnowyData snowyData, SnowSettings settings)
    {
        // Custom processing logic
        var browser = snowyData.Browser;
        var result = browser.Get("/custom-route");
        
        // Write output file
        File.WriteAllText(Path.Combine(Destination, "index.html"), result.Body.AsString());
    }
}
```

ProcessorFactory automatically discovers processors via reflection (classes derived from BaseProcessor).

### Custom Extensions
Add helper methods in the Extensions folder:

```csharp
public static class CustomHelpers
{
    public static string CustomFormat(this string input)
    {
        // Custom formatting logic
        return processed;
    }
}
```

## Deployment Options

### 1. Local Build
```bash
Snow.exe config=./MySite/
```
Generates static files to output directory.

### 2. Development Server  
```bash
Snow.exe config=./MySite/ server=true
```
Builds and serves on `http://localhost:5498` with OWIN.

### 3. Continuous Integration
- Azure WebJobs
- GitHub Actions
- TeamCity/Jenkins builds
- Any CI system supporting .NET executables

### 4. Barbato CaaS
Web application providing:
- GitHub webhook integration
- Automated builds on push
- FTP deployment to hosting providers
- User management and repository configuration

## Technology Stack

### Core Dependencies
- **.NET Framework 4.8**: Runtime platform
- **NancyFX 1.4.5**: Lightweight web framework
- **MarkdownSharp 1.13**: Markdown processing
- **Newtonsoft.Json 9.x**: JSON configuration handling
- **OWIN/Nowin**: Self-hosting web server
- **Microsoft.Owin.*** : OWIN middleware components

### Additional Libraries
- **CsQuery**: HTML manipulation and querying
- **xUnit**: Unit testing framework  
- **RestSharp**: HTTP client (Barbato)
- **NLog**: Logging (Barbato)
- **WorldDomination.Authentication**: OAuth providers (Barbato)

## Development Workflow

### Setting Up Development Environment
1. Clone repository:
   ```bash
   git clone https://github.com/Sandra/Sandra.Snow.git
   ```

2. Open `src/Sandra.Snow.sln` in Visual Studio

3. Configure debug arguments in Snow project properties:
   ```
   config=../../../../SnowSite/Snow/ debug=true server=true
   ```

4. Build and run (F5) - generates SnowSite and serves at localhost

### Creating Content
1. Add Markdown files to `SnowSite/Snow/_posts/` following naming convention
2. Create layouts in `SnowSite/Snow/themes/default/_layouts/`
3. Configure `snow.config.json` as needed
4. Run Snow.exe to build or use F5 for development server (opens http://localhost:5498)

### Testing
- **Snow.Tests**: Core engine unit tests using xUnit 2.x
- **Barbato.Tests**: Web application tests
- Run tests with Visual Studio Test Explorer or command line
- Tests can be run with `dotnet test` after migration to modern .NET

## Advanced Features

### Debug Capabilities
- `debug=true`: Enable console output and debugging helpers
- `vsdebug=true`: Wait for debugger attachment
- Conditional compilation for debug-only features

### Performance Considerations
- In-memory Nancy browser for template rendering
- Efficient file system operations with selective copying
- Lazy loading of content collections
- Markdown caching possibilities

### Security Features
- Front-matter validation and sanitization
- Safe HTML rendering with XSS protection
- Secure file path handling
- OAuth integration for Barbato admin

## Comparison with Jekyll

### Similarities
- Directory structure conventions (`_posts`, `_layouts`)
- Front matter YAML headers
- Markdown content processing
- Liquid-style templating concepts
- Static site generation workflow

### Differences
- **Language**: C#/.NET vs Ruby
- **Templates**: Razor vs Liquid
- **Extensions**: C# plugins vs Ruby gems  
- **Server**: OWIN vs WEBrick
- **Deployment**: Compiled executable vs Ruby runtime
- **IDE**: Visual Studio integration vs text editor focus

## Best Practices

### Content Organization
- Use consistent front matter across posts
- Implement proper category taxonomy
- Leverage series for related content
- Create meaningful excerpts for better UX

### Theme Development
- Follow responsive design principles
- Optimize assets (CSS/JS minification)
- Use semantic HTML markup
- Implement proper navigation structures

### Performance Optimization
- Minimize processor count in configuration
- Optimize image assets before copying
- Use efficient Razor template patterns
- Consider pagination for large content sets

### Maintenance
- Regular dependency updates
- Monitor build times and optimize as needed
- Implement automated backups of source content
- Version control theme customizations

## Troubleshooting

### Common Issues
1. **File naming**: Posts must match `YYYY-MM-DD-slug.md` format exactly
2. **Front matter**: YAML syntax errors break parsing
3. **Layouts**: Missing layout files cause rendering failures
4. **Paths**: Incorrect config paths prevent content discovery
5. **Dependencies**: Missing NuGet packages cause build failures

### Debug Process
1. Use `debug=true` flag for verbose output
2. Check Snow console output for specific errors
3. Verify file naming conventions
4. Validate JSON configuration syntax
5. Test with minimal content set first

### Performance Issues
- Large content sets: Consider pagination
- Complex templates: Profile rendering time
- Asset copying: Verify source paths exist
- Memory usage: Monitor with large image sets

## Future Considerations

### Potential Enhancements
- **Modern .NET**: Migration to .NET Core/5+ for cross-platform support
- **Template Engines**: RazorLight for .NET Core compatibility
- **Markdown Processing**: Upgrade to Markdig for extended syntax support
- **Configuration**: YAML/TOML support beyond JSON
- **Pagination**: Built-in multi-page navigation
- **Performance**: Incremental builds and caching
- **Themes**: Package manager for theme distribution

### Modernization Path
1. Assess Nancy → ASP.NET Core migration effort
2. Update to modern .NET runtime
3. Implement cross-platform compatibility
4. Add package manager integration
5. Enhance development tooling

## Conclusion

Sandra.Snow provides a powerful, developer-friendly alternative to Jekyll for .NET developers. Its combination of familiar tooling, strong typing, extensible architecture, and deployment flexibility makes it an excellent choice for static site generation in Microsoft-centric environments. The dual nature of supporting both simple local builds and sophisticated cloud-based compilation workflows provides scalability from personal blogs to enterprise documentation systems.

The project demonstrates solid software engineering principles with clear separation of concerns, comprehensive testing, and thoughtful configuration management. While some dependencies may benefit from modernization, the core architecture remains sound and the feature set comprehensive for static site generation needs.
