namespace Snow
{
    using Enums;
    using Models;
    using System.Collections.Generic;
    using System.Linq;

    public class CategoriesPage
    {
        public static List<Category> Create(IEnumerable<Post> posts)
        {
            var publishedPosts = posts.Where(ShouldProcess);

            var categories = (from c in publishedPosts.SelectMany(x => x.Categories)
                              group c by c
                              into g
                              select new Category
                              {
                                  Name = g.Key,
                                  Count = g.Count()
                              }).OrderBy(cat => cat.Name).ToList();

            var filteredCategories = categories.Where(ShouldProcess);

            return filteredCategories.ToList();
        }

        internal static bool ShouldProcess(Post post)
        {
            return post.Published == Published.True;
        }

        internal static bool ShouldProcess(Category category)
        {
            return category.Count > 0;
        }
    }
}