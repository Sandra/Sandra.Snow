namespace Barbato
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using FtpLib;
    using Model;
    using Nancy;
    using Nancy.ModelBinding;
    using RestSharp;

    public class IndexModule : NancyModule
    {
        private readonly IGithubUserRepository githubUserRepository;
        private readonly IRootPathProvider rootPathProvider;
        private readonly string gitLocation = ConfigurationManager.AppSettings["GitLocation"];
        private readonly string snowPublishPath = ConfigurationManager.AppSettings["SnowPublishFolder"];
        private readonly string snowPreCompilerPath;
        private string repoPath = ConfigurationManager.AppSettings["ClonedGitFolder"];
        private string fullRepoPath;
        private string fullPublishGitPath;
        private string publishGitPath;
        private FtpConnection ftp;

        public IndexModule(IGithubUserRepository githubUserRepository, IDeploymentRepository deploymentRepository, IRootPathProvider rootPathProvider)
        {
            this.githubUserRepository = githubUserRepository;
            this.rootPathProvider = rootPathProvider;

            this.snowPreCompilerPath = rootPathProvider.GetRootPath() + "Snow\\Snow.exe";

            Post["/"] = parameters =>
                {
                    var payloadModel = this.Bind<GithubHookModel.RootObject>();

                    //Check if user is registered
                    var githubhookfromUsername = payloadModel.repository.owner.name;
                    var githubhookfromRepo = payloadModel.repository.url;

                    if (!githubUserRepository.UserRegistered(githubhookfromUsername, githubhookfromRepo))
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

                    var request = new RestRequest("users/" + githubUser + "/repos?per_page=100");
                    request.AddHeader("Accept", "application/json");
                    request.AddHeader("User-Agent", githubUser);
                    var response = client.Execute<List<GithubUserRepos.RootObject>>(request);

                    var repoDetail =
                        response.Data
                        //.Where(x => x.fork == false)
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

                    DeployBlog(model);

                    deploymentRepository.AddDeployment(model);

                    Thread.Sleep(2500);

                    return "deployed";
                };

            Post["/alreadyregistered"] = parameters =>
                {
                    var model = this.Bind<AlreadyRegisteredModel>();

                    var alreadyRegistered = deploymentRepository.IsUserAndRepoRegistered(model.GitDeployment, model.Repo, model.Username);

                    var keys = new List<string>();
                    keys.Add(model.GitDeployment ? "gitrepo" : "ftpserver");

                    return new { isValid = !alreadyRegistered, keys = keys };
                };
        }

        private void DeployBlog(DeploymentModel model)
        {
            CloneFromGithub(model.CloneUrl, model.Username);

            CloneFromPublishLocation(model);

            LetItSnow();

            PublishToGitFTP(model);
            
            DeleteRepoPathContents(repoPath);
            Directory.Delete(repoPath);
        }

        private void CloneFromPublishLocation(DeploymentModel model)
        {
            if (model.GitDeployment)
            {
                publishGitPath = repoPath + "\\" + "Website";
                fullPublishGitPath = publishGitPath + "\\.git";

                CloneFromPublishGitRepository(model.GitRepo, publishGitPath);
            }
        }

        private void CloneFromGithub(string cloneUrl, string username)
        {
            var token = githubUserRepository.GetToken(username);
            if (token == string.Empty)
                throw new Exception("No auth token found for user " + username);

            token = token + "@";

            //Clone via https
            cloneUrl = cloneUrl.Insert(8, token);

            //Get repo name
            var lastSlashPos = cloneUrl.LastIndexOf("/", System.StringComparison.Ordinal) + 1;
            var repoName = cloneUrl.Substring(lastSlashPos,
                                              cloneUrl.LastIndexOf(".git", System.StringComparison.Ordinal) -
                                              lastSlashPos);
            repoName = repoName + DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            repoPath = repoPath + "\\" + repoName;
            fullRepoPath = repoPath + "\\.git";

            var cloneProcess =
                Process.Start(gitLocation, " clone " + cloneUrl + " " + repoPath);

            if (cloneProcess != null)
                cloneProcess.WaitForExit();

        }

        private void CloneFromPublishGitRepository(string repoUrl, string outputDir)
        {
            var cloneProcess =
               Process.Start(gitLocation, " clone " + repoUrl + " " + outputDir);

            if (cloneProcess != null)
                cloneProcess.WaitForExit();
        }

        private void LetItSnow()
        {
            var snowProcess = Process.Start(snowPreCompilerPath, " config=" + repoPath + "\\");
            if (snowProcess != null)
                snowProcess.WaitForExit();
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

        public void PublishToGitFTP(DeploymentModel model)
        {
            if (model.GitDeployment)
            {
                var addProcess = Process.Start("\"" + gitLocation + "\"", " --git-dir=\"" + fullPublishGitPath + "\" --work-tree=\"" + publishGitPath + "\" add -A");
                if (addProcess != null)
                    addProcess.WaitForExit();

                var commitProcess = Process.Start("\"" + gitLocation + "\"",
                                                  " --git-dir=\"" + fullPublishGitPath + "\" --work-tree=\"" + publishGitPath +
                                                  "\" commit -a -m \"Static Content Regenerated\"");
                if (commitProcess != null)
                    commitProcess.WaitForExit();


                var pushProcess = Process.Start("\"" + gitLocation + "\"", " --git-dir=\"" + fullPublishGitPath + "\" push -f origin master");
                if (pushProcess != null)
                    pushProcess.WaitForExit();

            }
            else
            {
                using (ftp = new FtpConnection(model.FTPServer, model.FTPUsername, model.FTPPassword))
                {
                    try
                    {
                        ftp.Open();
                        ftp.Login();

                        if (!string.IsNullOrWhiteSpace(model.FTPPath))
                        {
                            if (!ftp.DirectoryExists(model.FTPPath))
                            {
                                ftp.CreateDirectory(model.FTPPath);
                            }

                            ftp.SetCurrentDirectory(model.FTPPath);
                        }

                        FtpBlogFiles(snowPublishPath, model.FTPPath);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }

        private void FtpBlogFiles(string dirPath, string uploadPath)
        {
            string[] files = Directory.GetFiles(dirPath, "*.*");
            string[] subDirs = Directory.GetDirectories(dirPath);


            foreach (string file in files)
            {
                ftp.PutFile(file, Path.GetFileName(file));
            }

            foreach (string subDir in subDirs)
            {
                if (!ftp.DirectoryExists(uploadPath + "/" + Path.GetFileName(subDir)))
                {
                    ftp.CreateDirectory(uploadPath + "/" + Path.GetFileName(subDir));
                }

                ftp.SetCurrentDirectory(uploadPath + "/" + Path.GetFileName(subDir));

                FtpBlogFiles(subDir, uploadPath + "/" + Path.GetFileName(subDir));
            }
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
    }
}