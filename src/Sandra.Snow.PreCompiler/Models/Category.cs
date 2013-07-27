namespace Sandra.Snow.PreCompiler.Models
{
    public class Category
    {
        public Category(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
        public int Count { get; set; }

        public string Url
        {
            get { return Name.ToLower().Replace(" ", "-"); }
        }
    }
}