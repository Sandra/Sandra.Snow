namespace Sandra.Snow.Barbato.Model
{
    public interface IDeploymentRepository
    {
        bool IsUserAndRepoRegistered(bool azureDeployment, string repo, string username);
        void AddDeployment(DeploymentModel model);
        DeploymentModel GetDeployment(string username);
    }
}