namespace Sandra.Snow.Barbato
{
    using System.Collections.Generic;
    using System.Linq;
    using Model;

    public class GithubUserRepository : IGithubUserRepository
    {
        private static List<GithubUser> users = new List<GithubUser>();

        public bool UserRegistered(string userName, string repoUrl)
        {
            return users.Any(x => x.Username == userName && x.Repository == repoUrl);
        }

        public bool UserRegistered(string token)
        {
            return users.Any(x => x.Token == token);
        }

        public void AddOAuthToken(string token, string email, string username)
        {
            users.Add(new GithubUser() {Token = token, EMailAddress = email, Username = username});
        }

        public string GetToken(string username)
        {
            var firstOrDefault = users.FirstOrDefault(x => x.Username == username);
            if (firstOrDefault == null)
            {
                return string.Empty;
            }
            
            var token = firstOrDefault.Token;
            if (token == null)
            {
                return string.Empty;
            }
            
            return token;
        }
    }
}