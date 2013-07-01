namespace Sandra.Snow.Barbato.Tests
{
    using FakeItEasy;
    using Model;
    using Xunit;

    public class IndexModuleTests
    {
        [Fact]
        public void PublishToGitFTP_GitPassword_Added_Should_Be_Escaped()
        {
            var module = new IndexModule(A.Fake<IUserRepository>(), A.Fake<IDeploymentRepository>());

            var model = new DeploymentModel() { DeploymentType = "azure", AzureRepo = "https://jchannon:MyPassword!@myproj.scm.azurewebsites.net/myproj.git" };

            module.PublishToGitFTP(model);

            Assert.Equal("https://jchannon:\\M\\y\\P\\a\\s\\s\\w\\o\\r\\d\\!@myproj.scm.azurewebsites.net/myproj.git", model.AzureRepo);
        }
    }
}
