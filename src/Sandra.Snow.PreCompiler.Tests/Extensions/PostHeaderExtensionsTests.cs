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
                        Id = "123",
                        Parts = new SortedList<int, string>
                        {
                            { 1, "Part 1" },
                            { 2, "Part 2" },
                            { 3, "Part 3" },
                            { 4, "Part 4" },
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
                        Id = "123",
                        Parts = new SortedList<int, string>
                        {
                            { 1, "Test Part 1" },
                            { 2, "Test Part 2" },
                        }
                    }
                },
                new PostHeader
                {
                    Date = new DateTime(2013, 03, 10),
                    Series = new Series
                    {
                        Current = 1,
                        Id = "123",
                        Parts = new SortedList<int, string>
                        {
                            { 1, "Old Parts 1" },
                            { 2, "Old Parts 2" },
                            { 3, "Old Parts 3" },
                            { 4, "Old Parts 4" },
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

            Assert.Equal("Part 1", testData[0].Series.Parts[1]);
            Assert.Equal("Part 2", testData[0].Series.Parts[2]);
            Assert.Equal("Part 3", testData[0].Series.Parts[3]);
            Assert.Equal("Part 4", testData[0].Series.Parts[4]);

            Assert.Equal("Part 1", testData[2].Series.Parts[1]);
            Assert.Equal("Part 2", testData[2].Series.Parts[2]);
            Assert.Equal("Part 3", testData[2].Series.Parts[3]);
            Assert.Equal("Part 4", testData[2].Series.Parts[4]);

            Assert.Equal("Part 1", testData[3].Series.Parts[1]);
            Assert.Equal("Part 2", testData[3].Series.Parts[2]);
            Assert.Equal("Part 3", testData[3].Series.Parts[3]);
            Assert.Equal("Part 4", testData[3].Series.Parts[4]);
        }
    }
}
