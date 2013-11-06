namespace Snow.ViewModels
{
    using System;
    using Enums;
    using Models;

    public class PostViewModel : BaseViewModel
    {
        public string Layout { get; set; }
        public string Title { get; set; }
        public string PostContent { get; set; }
        public DateTime PostDate { get; set; }
        public string Url { get; set; }
              
        public string Email { get; set; }
        public string Author { get; set; }
              
        public SnowSettings Settings { get; set; }
        public Series Series { get; set; }
        public string MetaDescription { get; set; }
    }
}