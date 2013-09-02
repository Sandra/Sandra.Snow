namespace Sandra.Snow.PreCompiler.Models
{
    using Extensions;

    public class Category
    {
        public string Name { get; set; }
        public int Count { get; set; }

        public string Url
        {
            get { return Name.ToUrlSlug(); }
        }
    }
}