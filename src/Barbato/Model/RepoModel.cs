namespace Barbato.Model
{
    using System.Collections.Generic;

    public class RepoModel
    {
        public string Username { get; set; }
        public IEnumerable<RepoDetail> Repos { get; set; }
    }
    public class RepoDetail
    {
        public string CloneUrl { get; set; }

        public string UpdatedAt { get; set; }

        public string HtmlUrl { get; set; }

        public string Description { get; set; }

        public string AvatarUrl { get; set; }

        public string Name { get; set; }
    }
}