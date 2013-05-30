namespace Sandra.Snow.Barbato
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
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
        public IndexModule(IRootPathProvider rootPathProvider, IUserRepository userRepository, IDeploymentRepository deploymentRepository)
        {
            Post["/"] = parameters =>
                {
                    var payloadModel = this.Bind<GithubHookModel.RootObject>();

                    //Check if user is registered
                    var githubhookfromUsername = payloadModel.repository.owner.name;
                    var githubhookfromRepo = payloadModel.repository.url;

                    if (!userRepository.UserRegistered(githubhookfromUsername, githubhookfromRepo))
                        return HttpStatusCode.Forbidden;

                    var gitLocation = ConfigurationManager.AppSettings["GitLocation"];

                    var repoPath = rootPathProvider.GetRootPath() + ".git";
                    if (!Directory.Exists(repoPath))
                    {
                        var cloneProcess =
                            Process.Start(gitLocation + " clone " + payloadModel.repository.url + " " + repoPath);
                        if (cloneProcess != null)
                            cloneProcess.WaitForExit();
                    }
                    else
                    {
                        //Shell out to git.exe as LibGit2Sharp doesnt support Merge yet
                        var pullProcess =
                            Process.Start(gitLocation + " --git-dir=\"" + repoPath + "\" pull upstream master");
                        if (pullProcess != null)
                            pullProcess.WaitForExit();
                    }

                    //Run the PreCompiler

                    var addProcess = Process.Start(gitLocation + " --git-dir=\"" + repoPath + "\" add -A");
                    if (addProcess != null)
                        addProcess.WaitForExit();

                    var commitProcess =
                        Process.Start(gitLocation + " --git-dir=\"" + repoPath +
                                      "\" commit -a -m \"Static Content Regenerated\"");
                    if (commitProcess != null)
                        commitProcess.WaitForExit();

                    var pushProcess =
                        Process.Start("C:\\Program Files (x86)\\Git\bin\\git.exe --git-dir=\"" + repoPath +
                                      "\" push upstream master");
                    if (pushProcess != null)
                        pushProcess.WaitForExit();

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
                        
                    }

                   

                    return "deployed";
                };

            Post["/alreadyregistered"] = parameters =>
                {
                    string repo = (string) Request.Form.repo;

                    var alreadyRegistered = deploymentRepository.IsUserAndRepoRegistered();

                    return alreadyRegistered;
                };
        }
    }
}