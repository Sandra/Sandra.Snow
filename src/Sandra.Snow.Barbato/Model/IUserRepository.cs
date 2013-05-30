namespace Sandra.Snow.Barbato
{
    public interface IUserRepository
    {
        bool UserRegistered(string userName, string repoName);
    }
}