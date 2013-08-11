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

                //Fix up all the Parts to have the latest URLs if they exist
                foreach (var part in latestSeries.Parts)
                {
                    var post = f.FirstOrDefault(x => x.Series.Current == part.Key);

                    if (post != null)
                    {
                        part.Value.Url = post.Url;
                    }
                }

                //Assign the latest parts to all posts
                foreach (var ff in f)
                {
                    ff.Series.Parts = latestSeries.Parts;
                }
            }
        }
    }
}