namespace Sandra.Snow.PreCompiler.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using Models;

    public static class PostHeaderExtensions
    {
        public static void UpdatePartsToLatestInSeries(this IEnumerable<PostHeader> postHeaders)
        {
            var groupBySeriesId = (from x in postHeaders
                                  where x.Series != null
                                  group x by x.Series.Id
                                  into g
                                  select g).ToList();

            foreach (var f in groupBySeriesId)
            {
                var latestSeries = f.OrderByDescending(x => x.Date)
                    .Select(x => x.Series)
                    .First();

                foreach (var ff in f)
                {
                    ff.Series.Parts = latestSeries.Parts;
                }
            }
        }
    }
}