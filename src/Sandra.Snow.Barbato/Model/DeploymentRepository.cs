namespace Sandra.Snow.Barbato.Model
{
    public class DeploymentRepository : IDeploymentRepository
    {
        public bool IsUserAndRepoRegistered(bool azureDeployment, string repo, string username)
        {
            return true;
        }
    }
}