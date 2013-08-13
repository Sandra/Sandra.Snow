namespace Sandra.Snow.PreCompiler.ViewModels
{
    using System;
    using Models;

    public class PostViewModel : BaseViewModel
    {
        public virtual string Layout { get; set; }
        public virtual string Title { get; set; }
        public virtual string PostContent { get; set; }
        public virtual DateTime PostDate { get; set; }
        public virtual string Url { get; set; }
               
        public virtual string Email { get; set; }
        public virtual string Author { get; set; }
               
        public virtual SnowSettings Settings { get; set; }
        public Series Series { get; set; }
        public string MetaDescription { get; set; }
    }
}