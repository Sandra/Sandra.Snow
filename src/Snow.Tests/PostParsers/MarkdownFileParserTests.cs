namespace Snow.Tests.PostParsers
{
    using System.IO;
    using Snow.PostParsers;
    using Xunit;

    public class MarkdownFileParserTests
    {
        public class ParseDataFromFileTests
        {
            [Fact]
            public void Given_Location_To_Markdown_Files_Should_Be_Able_To_Open()
            {
                //Test exists to satistify my own ego to know if these files can be opened for testing. 
                var files = new[]
                {
                    "PostParsers/TestFiles/series-sample-test-1.md",
                    "PostParsers/TestFiles/series-sample-test-2.md",
                    "PostParsers/TestFiles/series-sample-test-3.md",
                    "PostParsers/TestFiles/series-sample-test-4.md",
                    "PostParsers/TestFiles/series-sample-test-5.md"
                };

                foreach (var s in files)
                {
                    var file = File.ReadAllText(s);

                    Assert.NotEqual("", file);
                }
            }

            [Fact]
            public void Given_File_Should_Return_Tuple_With_Item1_Containing_Header()
            {
                var fileData = File.ReadAllText("PostParsers/TestFiles/series-sample-test-1.md");
                const string expected = @"---
layout: post
series:
    name: 123
    current: 1
    part: test part 1
    part: test part 2
    part: test part 3
title: some title
---";

                var result = MarkdownFileParser.ParseDataFromFile(fileData);

                Assert.Equal(expected, result.Header);
            }

            [Fact]
            public void Given_File_Should_Return_Tuple_With_Item2_Containing_Post()
            {
                var fileData = File.ReadAllText("PostParsers/TestFiles/series-sample-test-1.md");
                const string expected = @"

Lorem ipsum dolor sit amet, consectetur adipiscing elit.

Donec porttitor non velit nec feugiat.";

                var result = MarkdownFileParser.ParseDataFromFile(fileData);

                Assert.Equal(expected, result.Body);
            }

            [Fact]
            public void Given_A_File_With_No_Header_Should_Return_Raw_Post_And_Empty_Header()
            {
                var fileData = File.ReadAllText("PostParsers/TestFiles/series-sample-test-2.md");
                const string expected = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit.

Donec porttitor non velit nec feugiat.";

                var result = MarkdownFileParser.ParseDataFromFile(fileData);

                Assert.Null(result.Header);
                Assert.Equal(expected, result.Body);
            }

            [Fact]
            public void Given_A_File_With_Header_ExcerptBlock_And_Body_Should_Create_Object_With_Correct_Data()
            {
                var fileData = File.ReadAllText("PostParsers/TestFiles/series-sample-test-6.md");
                const string expectedHeader = @"---
layout: post
series:
    name: 123
    current: 2
    part: test part 1
    part: test part 2
    part: test part 3
title: some title
---";
                const string expectedExcerpt = @"
Hello World, this is an excerpt
";
                const string expectedBody = @"

Hello rest of the world, this is the body...
";

                var result = MarkdownFileParser.ParseDataFromFile(fileData);

                Assert.Equal(expectedHeader, result.Header);
                Assert.Equal(expectedExcerpt, result.Excerpt);
                Assert.Equal(expectedBody, result.Body);
            }

//            [Fact]
//            public void Given_Empty_RawSettings_Should_Return_Empty_Dictionary()
//            {
//                var result = MarkdownFileParser.ParseSettings("");

//                Assert.Empty(result);
//            }

//            [Fact]
//            public void Given_Header_With_Empty_Settings_Should_Return_Empty_Dictionary()
//            {
//                var result = MarkdownFileParser.ParseSettings(@"---
//
//---");

//                Assert.Empty(result);
//            }

//            [Fact]
//            public void Given_Header_That_Contains_Two_Properties_With_Values_Should_Map_To_Dictionary_Correctly()
//            {
//                var fileData = File.ReadAllText("SettingParsers/TestFiles/series-sample-test-3.md");
//                var result = MarkdownFileParser.ParseSettings(fileData);

//                Assert.Equal("post", result["layout"]);
//                Assert.Equal("some title", result["title"]);
//            }

//            [Fact]
//            public void Given_Header_Which_Contains_Blank_Lines_Should_Only_Parse_Out_Valid_Lines_Correctly()
//            {
//                var fileData = File.ReadAllText("SettingParsers/TestFiles/series-sample-test-4.md");
//                var result = MarkdownFileParser.ParseSettings(fileData);

//                Assert.Equal("post", result["layout"]);
//                Assert.Equal("some title", result["title"]);
//                Assert.Equal(2, result.Count);
//            }

//            [Fact]
//            public void Given_Header_Which_Contains_Series_Should_Return_Series_Key_With_Series_Object()
//            {
//                var fileData = File.ReadAllText("SettingParsers/TestFiles/series-sample-test-5.md");
//                var result = MarkdownFileParser.ParseSettings(fileData);

//                Assert.True(result.ContainsKey("series"));

//                var series = (Series)result["series"];

//                Assert.NotEmpty(series.Parts);
//                Assert.Equal("123", series.Name);
//                Assert.Equal(2, series.Current);
//            }
        }

        public class ParseHeaderTests
        {
            
        }
    }
}
