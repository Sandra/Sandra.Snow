namespace Sandra.Snow.PreCompiler.Tests.SettingParsers
{
    using System.IO;
    using Xunit;

    public class PostParserTests
    {
        [Fact]
        public void Given_Location_To_Markdown_Files_Should_Be_Able_To_Open()
        {
            //Test exists to satistify my own ego to know if these files can be opened for testing. 
            var files = new[]
            {
                "SettingParsers/TestFiles/series-sample-test-1.md",
                "SettingParsers/TestFiles/series-sample-test-2.md",
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
            var fileData = File.ReadAllText("SettingParsers/TestFiles/series-sample-test-1.md");
            const string expected = @"---
layout: post
series:
    id: 123
    current: 1
    part: test part 1
    part: test part 2
    part: test part 3
title: some title
---";

            var result = PostParser.ParseDataFromFile(fileData);

            Assert.Equal(expected, result.Item1);
        }

        [Fact]
        public void Given_File_Should_Return_Tuple_With_Item2_Containing_Post()
        {
            var fileData = File.ReadAllText("SettingParsers/TestFiles/series-sample-test-1.md");
            const string expected = @"

Lorem ipsum dolor sit amet, consectetur adipiscing elit.

Donec porttitor non velit nec feugiat.";

            var result = PostParser.ParseDataFromFile(fileData);

            Assert.Equal(expected, result.Item2);
        }

        [Fact]
        public void Given_A_File_With_No_Header_Should_Return_Raw_Post_And_Empty_Header()
        {
            var fileData = File.ReadAllText("SettingParsers/TestFiles/series-sample-test-2.md");
            const string expected = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit.

Donec porttitor non velit nec feugiat.";

            var result = PostParser.ParseDataFromFile(fileData);

            Assert.Equal(string.Empty, result.Item1);
            Assert.Equal(expected, result.Item2);
        }
    }
}
