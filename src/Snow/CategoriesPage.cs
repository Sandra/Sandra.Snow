namespace Snow
{
    using Models;
    using System.Collections.Generic;
    using System.Linq;

    public class CategoriesPage
    {
        public static List<Category> Create(IEnumerable<Post> posts)
        {
            var publishedPosts = posts.Where(ShouldProcess.Category);

            var categories = (from c in publishedPosts.SelectMany(x => x.Categories)
                              group c by c
                                  into g
                                  select new Category
                                  {
                                      Name = g.Key,
                                      Count = g.Count()
                                  }).OrderBy(cat => cat.Name).ToList();

            var filteredCategories = categories.Where(ShouldProcess.Category);

            var distinctCategories =
                filteredCategories.GroupBy(x => x.Name.ToLower()).Select(group => @group.Last()).ToList();

            return distinctCategories;
        }
    }
}