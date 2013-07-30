namespace Sandra.Snow.PreCompiler.ViewModels
{
    using System.Collections.Generic;

    public class PostViewModel : BaseViewModel
    {
        public string Layout { get; set; }
        public string Title { get; set; }
        public string PostContent { get; set; }
        public string PostDate { get; set; }
        public string Url { get; set; }
        
        public IEnumerable<Category> Categories { get; set; }
    }
}