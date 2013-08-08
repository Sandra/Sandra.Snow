namespace Sandra.Snow.PreCompiler.ViewModels
{
    using System;
    using System.Collections.Generic;

    public class PostViewModel : BaseViewModel
    {
        public virtual string Layout { get; set; }
        public virtual string Title { get; set; }
        public virtual string PostContent { get; set; }
        public virtual DateTime PostDate { get; set; }
        public virtual string Url { get; set; }
               
        public virtual string Email { get; set; }
        public virtual string Author { get; set; }
               
        public virtual IEnumerable<Category> Categories { get; set; }
        public virtual SnowSettings Settings { get; set; }
    }
}