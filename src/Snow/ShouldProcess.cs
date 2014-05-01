namespace Snow
{
    using Enums;
    using Models;

    public static class ShouldProcess
    {
        public static bool Archive(Post post)
        {
            return IsPublished(post);
        }

        public static bool Category(Post post)
        {
            return IsPublished(post);
        }

        public static bool Category(Category category)
        {
            return category.Count > 0;
        }

        public static bool Feed(Post post)
        {
            return IsPublished(post);
        }

        public static bool Posts(Post post)
        {
            return IsPublished(post);
        }

        private static bool IsPublished(Post post)
        {
            return post.Published == Published.True;
        }
    }
}