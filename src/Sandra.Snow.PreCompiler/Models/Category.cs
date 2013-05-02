namespace Sandra.Snow.PreCompiler
{
    public class Category
    {
        public Category(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public string Url
        {
            get { return Name.ToLower().Replace(" ", "-"); }
        }
    }
}