namespace Snow
{
    using Extensions;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ViewModels;

    public static class ArchiveMenu
    {
        public static List<BaseViewModel.MonthYear> Create(IEnumerable<Post> posts)
        {
            var filteredPosts = posts.Where(ShouldProcess.Archive).ToList();
            var groupedByYear = GroupMonthYearArchive(filteredPosts);

            return BuildViewModel(groupedByYear);
        }

        internal static List<BaseViewModel.MonthYear> BuildViewModel(Dictionary<DateTime, Dictionary<DateTime, int>> groupedByYear)
        {
            return (from s in groupedByYear
                    from y in s.Value
                    select new BaseViewModel.MonthYear
                    {
                        Count = y.Value,
                        Title = y.Key.ToString("MMMM, yyyy"),
                        Url = "/archive#" + y.Key.ToString("yyyyM")
                    }).ToList();
        }

        internal static Dictionary<DateTime, Dictionary<DateTime, int>> GroupMonthYearArchive(IEnumerable<Post> parsedFiles)
        {
            var groupedByYear = (from p in parsedFiles
                                 group p by p.Date.AsYearDate()
                                     into g
                                     select g).ToDictionary(x => x.Key, x => (from y in x
                                                                              group y by y.Date.AsMonthDate()
                                                                                  into p
                                                                                  select p).ToDictionary(u => u.Key,
                                                                              u => u.Count()));

            return groupedByYear;
        }
    }
}