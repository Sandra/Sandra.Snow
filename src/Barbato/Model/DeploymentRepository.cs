namespace Barbato.Model
{
    using System.Collections.Generic;
    using System.Linq;

    public class DeploymentRepository : IDeploymentRepository
    {
        private static List<DeploymentModel> deployments = new List<DeploymentModel>();

        public bool IsUserAndRepoRegistered(bool azureDeployment, string repo, string username)
        {
            return
                deployments.Any(
                    x => azureDeployment ? x.AzureRepo == repo : x.FTPServer == repo && x.Username == username);
        }

        public void AddDeployment(DeploymentModel model)
        {
            deployments.Add(model);
        }

        public DeploymentModel GetDeployment(string username)
        {
            return deployments.FirstOrDefault(x => x.Username == username);
        }
    }
}