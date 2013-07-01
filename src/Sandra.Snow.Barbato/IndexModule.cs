namespace Sandra.Snow.Barbato
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using Nancy;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using Nancy.ModelBinding;
    using Nancy.Validation;
    using RestSharp;
    using Sandra.Snow.Barbato.Model;

    public class IndexModule : NancyModule
    {
        private readonly IUserRepository userRepository;
        private readonly string repoPath = ConfigurationManager.AppSettings["ClonedGitFolder"];
        private readonly string gitLocation = ConfigurationManager.AppSettings["GitLocation"];
        private readonly string fullRepoPath = ConfigurationManager.AppSettings["ClonedGitFolder"] + "\\.git";

        public IndexModule(IUserRepository userRepository, IDeploymentRepository deploymentRepository)
        {
            this.userRepository = userRepository;

            Post["/"] = parameters =>
                {
                    var payloadModel = this.Bind<GithubHookModel.RootObject>();

                    //Check if user is registered
                    var githubhookfromUsername = payloadModel.repository.owner.name;
                    var githubhookfromRepo = payloadModel.repository.url;

                    if (!userRepository.UserRegistered(githubhookfromUsername, githubhookfromRepo))
                        return HttpStatusCode.Forbidden;

                    var deploymentModel = deploymentRepository.GetDeployment(githubhookfromUsername);

                    DeployBlog(deploymentModel);

                    return 200;
                };

            Get["/"] = parameters => { return View["Index"]; };

            Get["/repos"] = parameters =>
            {
                return View["Repos"];
            };

            Get["getrepodata/{githubuser}"] = parameters =>
                {
                    var githubUser = (string)parameters.githubuser;

                    var client = new RestClient("https://api.github.com");

                    var request = new RestRequest("users/" + githubUser + "/repos");
                    request.AddHeader("Accept", "application/json");

                    var response = client.Execute<List<GithubUserRepos.RootObject>>(request);

                    var repoDetail =
                        response.Data
                        .Where(x => x.fork == false)
                        .Select(
                            x =>
                            new RepoDetail
                            {
                                Name = x.name,
                                AvatarUrl = x.owner.avatar_url,
                                Description = x.description,
                                HtmlUrl = x.html_url,
                                UpdatedAt = DateTime.Parse(x.pushed_at).ToRelativeTime(),
                                CloneUrl = x.clone_url
                            });

                    var viewModel = new RepoModel { Username = githubUser, Repos = repoDetail };

                    return viewModel;
                };

            Post["initializedeployment"] = parameters =>
                {
                    var model = this.BindAndValidate<DeploymentModel>();
                    if (!this.ModelValidationResult.IsValid)
                    {
                        return 400;
                    }

                    deploymentRepository.AddDeployment(model);

                    DeployBlog(model);

                    Thread.Sleep(2500);

                    return "deployed";
                };

            Post["/alreadyregistered"] = parameters =>
                {
                    var model = this.Bind<AlreadyRegisteredModel>();

                    var alreadyRegistered = deploymentRepository.IsUserAndRepoRegistered(model.AzureDeployment, model.Repo, model.Username);

                    var keys = new List<string>();
                    keys.Add(model.AzureDeployment ? "azurerepo" : "ftpserver");

                    return new { isValid = !alreadyRegistered, keys = keys };
                };
        }

        private void DeployBlog(DeploymentModel model)
        {
            CloneFromGithub(model.CloneUrl, model.Username);

            LetItSnow();

            PushToGithub();

            PublishToGitFTP(model);

            DeleteRepoPathContents(repoPath);
        }

        private void CloneFromGithub(string cloneUrl, string username)
        {
            var token = userRepository.GetToken(username);
            if (token == string.Empty)
                throw new Exception("No auth token found for user " + username);

            token = token + "@";

            //Clone via https
            cloneUrl = cloneUrl.Insert(8, token);

            var cloneProcess =
                Process.Start(gitLocation, " clone " + cloneUrl + " " + repoPath);

            if (cloneProcess != null)
                cloneProcess.WaitForExit();
        }

        private void LetItSnow()
        {
        }

        private void PushToGithub()
        {
            var addProcess = Process.Start("\"" + gitLocation + "\"", " --git-dir=\"" + fullRepoPath + "\" --work-tree=\"" + repoPath + "\" add -A");
            if (addProcess != null)
                addProcess.WaitForExit();

            var commitProcess =
                Process.Start("\"" + gitLocation + "\"", " --git-dir=\"" + fullRepoPath + "\" --work-tree=\"" + repoPath + "\" commit -a -m \"Static Content Regenerated\"");
            if (commitProcess != null)
                commitProcess.WaitForExit();

            var pushProcess =
                Process.Start("\"" + gitLocation + "\"", " --git-dir=\"" + fullRepoPath + "\" --work-tree=\"" + repoPath + "\" push origin master");
            if (pushProcess != null)
                pushProcess.WaitForExit();

           
        }

        private void DeleteRepoPathContents(string folderName)
        {
            var repoPathDir = new DirectoryInfo(folderName);

            foreach (FileInfo fi in repoPathDir.GetFiles())
            {
                fi.IsReadOnly = false;
                fi.Delete();
            }

            foreach (DirectoryInfo di in repoPathDir.GetDirectories())
            {
                DeleteRepoPathContents(di.FullName);
                di.Delete();
            }
        }

        private void PublishToGitFTP(DeploymentModel model)
        {
            if (model.AzureDeployment)
            {
                var remoteProcess =
                     Process.Start("\"" + gitLocation + "\"", " --git-dir=\"" + fullRepoPath + "\" remote add blog " + model.AzureRepo);
                if (remoteProcess != null)
                    remoteProcess.WaitForExit();

                var pushProcess = Process.Start("\"" + gitLocation + "\"", " --git-dir=\"" + fullRepoPath + "\" push blog master");
                if (pushProcess != null)
                    pushProcess.WaitForExit();

            }
            else
            {

            }
        }
    }
}