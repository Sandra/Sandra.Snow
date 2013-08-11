namespace Sandra.Snow.PreCompiler.Tests.Extensions
{
    using System;
    using System.Collections.Generic;
    using Models;
    using PreCompiler.Extensions;
    using Xunit;

    public class PostHeaderExtensionsTests
    {
        [Fact]
        public void Given_Empty_Collection_Should_Do_Nothing()
        {
            var testData = new List<PostHeader>();

            testData.UpdatePartsToLatestInSeries();
        }

        [Fact]
        public void Given_Collection_With_No_Series_Should_Not_Throw()
        {
            var testData = new List<PostHeader>
            {
                new PostHeader(),
                new PostHeader(),
                new PostHeader(),
                new PostHeader(),
            };

            testData.UpdatePartsToLatestInSeries();
        }

        [Fact]
        public void Given_Collection_With_Series_Should_Update_To_Latest_Parts_On_All()
        {
            var testData = new List<PostHeader>
            {
                new PostHeader
                {
                    Date = new DateTime(2013, 03, 28),
                    Series = new Series
                    {
                        Current = 3,
                        Name = "123",
                        Parts = new SortedList<int, Series.Part>
                        {
                            { 1, new Series.Part { Name = "Part 1" }},
                            { 2, new Series.Part { Name = "Part 2" }},
                            { 3, new Series.Part { Name = "Part 3" }},
                            { 4, new Series.Part { Name = "Part 4" }},
                        }
                    }
                },
                new PostHeader(),
                new PostHeader
                {
                    Date = new DateTime(2013, 03, 14),
                    Series = new Series
                    {
                        Current = 2,
                        Name = "123",
                        Parts = new SortedList<int, Series.Part>
                        {
                            { 1, new Series.Part { Name = "Test Part 1" }},
                            { 2, new Series.Part { Name = "Test Part 2" }},
                        }
                    }
                },
                new PostHeader
                {
                    Date = new DateTime(2013, 03, 10),
                    Series = new Series
                    {
                        Current = 1,
                        Name = "123",
                        Parts = new SortedList<int, Series.Part>
                        {
                            { 1, new Series.Part { Name = "Old Parts 1" }},
                            { 2, new Series.Part { Name = "Old Parts 2" }},
                            { 3, new Series.Part { Name = "Old Parts 3" }},
                            { 4, new Series.Part { Name = "Old Parts 4" }},
                        }
                    }
                },
            };

            testData.UpdatePartsToLatestInSeries();

            Assert.Equal(4, testData[0].Series.Parts.Count);
            Assert.Equal(4, testData[2].Series.Parts.Count);
            Assert.Equal(4, testData[3].Series.Parts.Count);

            Assert.Equal(3, testData[0].Series.Current);
            Assert.Equal(2, testData[2].Series.Current);
            Assert.Equal(1, testData[3].Series.Current);

            Assert.Equal("Part 1", testData[0].Series.Parts[1].Name);
            Assert.Equal("Part 2", testData[0].Series.Parts[2].Name);
            Assert.Equal("Part 3", testData[0].Series.Parts[3].Name);
            Assert.Equal("Part 4", testData[0].Series.Parts[4].Name);

            Assert.Equal("Part 1", testData[2].Series.Parts[1].Name);
            Assert.Equal("Part 2", testData[2].Series.Parts[2].Name);
            Assert.Equal("Part 3", testData[2].Series.Parts[3].Name);
            Assert.Equal("Part 4", testData[2].Series.Parts[4].Name);

            Assert.Equal("Part 1", testData[3].Series.Parts[1].Name);
            Assert.Equal("Part 2", testData[3].Series.Parts[2].Name);
            Assert.Equal("Part 3", testData[3].Series.Parts[3].Name);
            Assert.Equal("Part 4", testData[3].Series.Parts[4].Name);
        }

        [Fact]
        public void Given_Collection_Of_Series_Should_Update_Parts_With_Post_Urls()
        {
            var testData = new List<PostHeader>
            {
                new PostHeader
                {
                    Url = "/2013/03/part-3-banana",
                    Date = new DateTime(2013, 03, 28),
                    Series = new Series
                    {
                        Current = 3,
                        Name = "123",
                        Parts = new SortedList<int, Series.Part>
                        {
                            { 1, new Series.Part { Name = "Part 1" }},
                            { 2, new Series.Part { Name = "Part 2" }},
                            { 3, new Series.Part { Name = "Part 3" }},
                            { 4, new Series.Part { Name = "Part 4" }},
                        }
                    }
                },
                new PostHeader
                {
                    Url = "/2013/03/part-2-orange",
                    Date = new DateTime(2013, 03, 14),
                    Series = new Series
                    {
                        Current = 2,
                        Name = "123",
                        Parts = new SortedList<int, Series.Part>
                        {
                            { 1, new Series.Part { Name = "Test Part 1" }},
                            { 2, new Series.Part { Name = "Test Part 2" }},
                        }
                    }
                },
                new PostHeader
                {
                    Url = "/2013/03/part-1-apple",
                    Date = new DateTime(2013, 03, 10),
                    Series = new Series
                    {
                        Current = 1,
                        Name = "123",
                        Parts = new SortedList<int, Series.Part>
                        {
                            { 1, new Series.Part { Name = "Old Parts 1" }},
                            { 2, new Series.Part { Name = "Old Parts 2" }},
                            { 3, new Series.Part { Name = "Old Parts 3" }},
                            { 4, new Series.Part { Name = "Old Parts 4" }},
                        }
                    }
                },
            };

            testData.UpdatePartsToLatestInSeries();

            Assert.Equal("/2013/03/part-1-apple", testData[0].Series.Parts[1].Url);
            Assert.Equal("/2013/03/part-2-orange", testData[0].Series.Parts[2].Url);
            Assert.Equal("/2013/03/part-3-banana", testData[0].Series.Parts[3].Url);
            Assert.False(testData[0].Series.Parts[4].HasUrl());
        }
    }
}