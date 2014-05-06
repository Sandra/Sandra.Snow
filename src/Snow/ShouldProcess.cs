namespace Snow
{
    using Enums;
    using Models;
    using System;

    public static class ShouldProcess
    {
        public static bool Archive(Post post)
        {
            return IsPublished(post) && IsInThePast(post);
        }

        public static bool Category(Post post)
        {
            return IsPublished(post) && IsInThePast(post);
        }

        public static bool Category(Category category)
        {
            return category.Count > 0;
        }

        public static bool Feed(Post post)
        {
            return IsPublished(post) && IsInThePast(post);
        }

        public static bool Posts(Post post)
        {
            return IsPublished(post) && IsInThePast(post);
        }

        private static bool IsInThePast(Post post)
        {
            return post.Date < DateTime.Now;
        }

        private static bool IsPublished(Post post)
        {
            return post.Published == Published.True;
        }
    }
}