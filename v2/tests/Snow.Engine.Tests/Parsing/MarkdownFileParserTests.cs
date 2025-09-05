using FluentAssertions;
using Snow.Engine.Parsing;
using Xunit;

namespace Snow.Engine.Tests.Parsing;

public class MarkdownFileParserTests
{
    [Fact]
    public void ParseDataFromFile_WithEmptyContent_ShouldReturnEmptyBody()
    {
        // Act
        var result = MarkdownFileParser.ParseDataFromFile("");

        // Assert
        result.Should().NotBeNull();
        result.Header.Should().BeEmpty();
        result.Body.Should().BeEmpty();
        result.Excerpt.Should().BeEmpty();
    }

    [Fact]
    public void ParseDataFromFile_WithOnlyBody_ShouldReturnBodyOnly()
    {
        // Arrange
        var content = "This is just markdown content without front matter.";

        // Act
        var result = MarkdownFileParser.ParseDataFromFile(content);

        // Assert
        result.Should().NotBeNull();
        result.Header.Should().BeEmpty();
        result.Body.Should().Be(content);
        result.Excerpt.Should().BeEmpty();
    }

    [Fact]
    public void ParseDataFromFile_WithFrontMatter_ShouldSeparateHeaderAndBody()
    {
        // Arrange
        var content = @"---
title: My Post
author: John Doe
---
This is the body content.

With multiple paragraphs.";

        // Act
        var result = MarkdownFileParser.ParseDataFromFile(content);

        // Assert
        result.Should().NotBeNull();
        result.Header.Should().Be(@"---
title: My Post
author: John Doe
---");
        result.Body.Should().Be(@"This is the body content.

With multiple paragraphs.");
        result.Excerpt.Should().BeEmpty();
    }

    [Fact]
    public void ParseDataFromFile_WithExcerptSection_ShouldExtractExcerpt()
    {
        // Arrange
        var content = @"---
title: My Post
---

---excerpt
This is the excerpt content.
It can span multiple lines.
---end

This is the main body content after the excerpt.

More body content here.";

        // Act
        var result = MarkdownFileParser.ParseDataFromFile(content);

        // Assert
        result.Should().NotBeNull();
        result.Header.Should().Be(@"---
title: My Post
---");
        result.Excerpt.Should().Be(@"This is the excerpt content.
It can span multiple lines.");
        result.Body.Should().Be(@"This is the main body content after the excerpt.

More body content here.");
    }

    [Fact]
    public void ParseDataFromFile_WithIncompleteExcerptSection_ShouldIgnoreExcerpt()
    {
        // Arrange
        var content = @"---
title: My Post
---

---excerpt
This excerpt has no end marker.

Body content continues.";

        // Act
        var result = MarkdownFileParser.ParseDataFromFile(content);

        // Assert
        result.Should().NotBeNull();
        result.Header.Should().Be(@"---
title: My Post
---");
        result.Body.Should().Be(@"---excerpt
This excerpt has no end marker.

Body content continues.");
        result.Excerpt.Should().BeEmpty();
    }

    [Fact]
    public void ParseDataFromFile_WithOnlyOpeningFrontMatter_ShouldTreatAsBody()
    {
        // Arrange
        var content = @"---
title: My Post
author: John Doe

This looks like front matter but has no closing delimiter.";

        // Act
        var result = MarkdownFileParser.ParseDataFromFile(content);

        // Assert
        result.Should().NotBeNull();
        result.Header.Should().BeEmpty();
        result.Body.Should().Be(content);
        result.Excerpt.Should().BeEmpty();
    }

    [Fact]
    public void ParseDataFromFile_WithComplexContent_ShouldParseAllComponents()
    {
        // Arrange
        var content = @"---
published: published
layout: post
title: Complex Post Example
---

---excerpt
This is a custom excerpt that will be used
in post listings instead of the automatic excerpt.
---end

# Main Content

This is the main body of the post.

## Section 1

Content here.

## Section 2

More content.";

        // Act
        var result = MarkdownFileParser.ParseDataFromFile(content);

        // Assert
        result.Should().NotBeNull();
        result.Header.Should().Be(@"---
published: published
layout: post
title: Complex Post Example
---");
        result.Excerpt.Should().Be(@"This is a custom excerpt that will be used
in post listings instead of the automatic excerpt.");
        result.Body.Should().Be(@"# Main Content

This is the main body of the post.

## Section 1

Content here.

## Section 2

More content.");
    }
}
