namespace Sandra.Snow.Barbato
{
    public interface IUserRepository
    {
        bool UserRegistered(string userName, string repoName);
        bool UserRegistered(string token);
        void AddOAuthToken(string token, string email, string username);
        string GetToken(string username);
    }
}