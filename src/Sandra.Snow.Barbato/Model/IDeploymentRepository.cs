namespace Sandra.Snow.Barbato.Model
{
    public interface IDeploymentRepository
    {
        bool IsUserAndRepoRegistered(bool azureDeployment, string repo, string username);
    }
}