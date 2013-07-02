namespace Sandra.Snow.Barbato.Model
{
    using System.ComponentModel.DataAnnotations;

    public class DeploymentModel
    {
        [Required]
        public string CloneUrl { get; set; }

        public bool AzureDeployment
        {
            get { return DeploymentType == "azure"; }
        }
        
        public string DeploymentType { get; set; }
        
        public string AzureRepo { get; set; }

        public string FTPServer { get; set; }

        public string FTPPath { get; set; }

        public string FTPUsername { get; set; }

        public string FTPPassword { get; set; }

        [Required]
        public string Username { get; set; }
    }
}