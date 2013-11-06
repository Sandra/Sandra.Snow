namespace Snow
{
    using Enums;
    using Models;
    using System.Collections.Generic;
    using System.Linq;

    public static class ArchivePage
    {
        public static Dictionary<int, Dictionary<int, List<Post>>> Create(IEnumerable<Post> posts)
        {
            var filteredPosts = posts.Where(ShouldProcess).ToList();

            var groupedByYear = GroupByYear(filteredPosts);

            return groupedByYear;
        }

        private static Dictionary<int, List<Post>> GroupByMonth(IEnumerable<Post> x)
        {
            return (from y in x
                    group y by y.Month
                    into p
                    select p).ToDictionary(u => u.Key, u => u.ToList());
        }

        private static Dictionary<int, Dictionary<int, List<Post>>> GroupByYear(IEnumerable<Post> filteredPosts)
        {
            return (from p in filteredPosts
                    group p by p.Year
                    into g
                    select g).ToDictionary(x => x.Key, GroupByMonth);
        }

        internal static bool ShouldProcess(Post post)
        {
            return post.Published == Published.True;
        }
    }
}