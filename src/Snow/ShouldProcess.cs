namespace Snow
{
    using Enums;
    using Models;

    public static class ShouldProcess
    {
        public static bool Archive(Post post)
        {
            return post.Published == Published.True;
        }

        public static bool Category(Post post)
        {
            return post.Published == Published.True;
        }

        public static bool Category(Category category)
        {
            return category.Count > 0;
        }

        public static bool Feed(Post post)
        {
            return post.Published == Published.True;
        }

        public static bool Posts(Post post)
        {
            return post.Published == Published.True;
        }
    }
}