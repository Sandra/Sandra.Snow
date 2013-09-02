namespace Barbato.Model
{
    public interface IGithubUserRepository
    {
        bool UserRegistered(string userName, string repoUrl);
        bool UserRegistered(string token);
        void AddOAuthToken(string token, string email, string username);
        string GetToken(string username);
    }
}