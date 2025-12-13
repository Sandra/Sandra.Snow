using System.Collections.Generic;
using FluentAssertions;
using Snow.Engine.Models;
using Snow.Engine.Parsing;
using Xunit;

namespace Snow.Engine.Tests.Parsing;

public class FrontMatterParserTests
{
    [Fact]
    public void ParseSettings_WithEmptyInput_ShouldReturnEmptyDictionary()
    {
        // Act
        var result = FrontMatterParser.ParseSettings("");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseSettings_WithNullInput_ShouldReturnEmptyDictionary()
    {
        // Act
        var result = FrontMatterParser.ParseSettings(null);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseSettings_WithSimpleKeyValuePairs_ShouldParseCorrectly()
    {
        // Arrange
        var input = @"---
title: My Blog Post
author: John Doe
published: true
---";

        // Act
        var result = FrontMatterParser.ParseSettings(input);

        // Assert
        result.Should().HaveCount(3);
        result["title"].Should().Be("My Blog Post");
        result["author"].Should().Be("John Doe");
        result["published"].Should().Be("true");
    }

    [Fact]
    public void ParseSettings_WithQuotedValues_ShouldRemoveQuotes()
    {
        // Arrange
        var input = @"---
title: ""Quoted Title""
description: 'Single quoted description'
---";

        // Act
        var result = FrontMatterParser.ParseSettings(input);

        // Assert
        result.Should().HaveCount(2);
        result["title"].Should().Be("Quoted Title");
        result["description"].Should().Be("Single quoted description");
    }

    [Fact]
    public void ParseSettings_WithSeries_ShouldParseSeries()
    {
        // Arrange
        var input = @"---
title: Part 1 of Series
series:
    name: My Series
    current: 1
    part: Introduction
    part: Getting Started
    part: Advanced Topics
---";

        // Act
        var result = FrontMatterParser.ParseSettings(input);

        // Assert
        result.Should().HaveCount(2);
        result["title"].Should().Be("Part 1 of Series");
        
        var series = result["series"].Should().BeOfType<Series>().Subject;
        series.Name.Should().Be("My Series");
        series.Current.Should().Be(1);
        series.Parts.Should().HaveCount(3);
        series.Parts[1].Name.Should().Be("Introduction");
        series.Parts[2].Name.Should().Be("Getting Started");
        series.Parts[3].Name.Should().Be("Advanced Topics");
    }

    [Fact]
    public void ParseSettings_WithMixedContent_ShouldParseAll()
    {
        // Arrange
        var input = @"---
published: published
layout: post
category: Programming
title: My Blog Post Title
author: Custom Author
series:
    name: ""My Series""
    current: 1
    part: First Part
    part: Second Part
tags: programming, csharp, web
---";

        // Act
        var result = FrontMatterParser.ParseSettings(input);

        // Assert
        result.Should().HaveCount(7);
        result["published"].Should().Be("published");
        result["layout"].Should().Be("post");
        result["category"].Should().Be("Programming");
        result["title"].Should().Be("My Blog Post Title");
        result["author"].Should().Be("Custom Author");
        result["tags"].Should().Be("programming, csharp, web");
        
        var series = result["series"].Should().BeOfType<Series>().Subject;
        series.Name.Should().Be("My Series");
        series.Current.Should().Be(1);
        series.Parts.Should().HaveCount(2);
    }

    [Fact]
    public void ParseSettings_WithEmptyLines_ShouldIgnoreThem()
    {
        // Arrange
        var input = @"---
title: My Title

author: My Author

layout: post
---";

        // Act
        var result = FrontMatterParser.ParseSettings(input);

        // Assert
        result.Should().HaveCount(3);
        result["title"].Should().Be("My Title");
        result["author"].Should().Be("My Author");
        result["layout"].Should().Be("post");
    }
}
