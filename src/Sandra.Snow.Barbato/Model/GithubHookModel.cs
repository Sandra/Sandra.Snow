using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sandra.Snow.Barbato.Model
{
    public class GithubHookModel
    {
        //JSON2CSharp from https://gist.github.com/gjtorikian/5171861/raw/156ee21193715a9543aa4cc8080d1e745aeab1ba/sample_payload.json via
        //https://help.github.com/articles/post-receive-hooks

        public class Author
        {
            public string email { get; set; }
            public string name { get; set; }
            public string username { get; set; }
        }

        public class Committer
        {
            public string email { get; set; }
            public string name { get; set; }
            public string username { get; set; }
        }

        public class Commit
        {
            public List<object> added { get; set; }
            public Author author { get; set; }
            public Committer committer { get; set; }
            public bool distinct { get; set; }
            public string id { get; set; }
            public string message { get; set; }
            public List<object> modified { get; set; }
            public List<object> removed { get; set; }
            public string timestamp { get; set; }
            public string url { get; set; }
        }

        public class Author2
        {
            public string email { get; set; }
            public string name { get; set; }
            public string username { get; set; }
        }

        public class Committer2
        {
            public string email { get; set; }
            public string name { get; set; }
            public string username { get; set; }
        }

        public class HeadCommit
        {
            public List<string> added { get; set; }
            public Author2 author { get; set; }
            public Committer2 committer { get; set; }
            public bool distinct { get; set; }
            public string id { get; set; }
            public string message { get; set; }
            public List<object> modified { get; set; }
            public List<string> removed { get; set; }
            public string timestamp { get; set; }
            public string url { get; set; }
        }

        public class Pusher
        {
            public string name { get; set; }
        }

        public class Owner
        {
            public string email { get; set; }
            public string name { get; set; }
        }

        public class Repository
        {
            public int created_at { get; set; }
            public string description { get; set; }
            public bool fork { get; set; }
            public int forks { get; set; }
            public bool has_downloads { get; set; }
            public bool has_issues { get; set; }
            public bool has_wiki { get; set; }
            public string homepage { get; set; }
            public int id { get; set; }
            public string language { get; set; }
            public string master_branch { get; set; }
            public string name { get; set; }
            public int open_issues { get; set; }
            public Owner owner { get; set; }
            public bool @private { get; set; }
            public int pushed_at { get; set; }
            public int size { get; set; }
            public int stargazers { get; set; }
            public string url { get; set; }
            public int watchers { get; set; }
        }

        public class RootObject
        {
            public string after { get; set; }
            public string before { get; set; }
            public List<Commit> commits { get; set; }
            public string compare { get; set; }
            public bool created { get; set; }
            public bool deleted { get; set; }
            public bool forced { get; set; }
            public HeadCommit head_commit { get; set; }
            public Pusher pusher { get; set; }
            public string @ref { get; set; }
            public Repository repository { get; set; }
        }
    }
}