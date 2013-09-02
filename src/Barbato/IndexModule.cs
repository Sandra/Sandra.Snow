﻿namespace Barbato
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
        private readonly string repoPath = ConfigurationManager.AppSettings["ClonedGitFolder"];
        private readonly string gitLocation = ConfigurationManager.AppSettings["GitLocation"];
        private readonly string fullRepoPath = ConfigurationManager.AppSettings["ClonedGitFolder"] + "\\.git";
        private readonly string snowPublishPath = ConfigurationManager.AppSettings["SnowPublishFolder"];
        private readonly string snowPreCompilerPath;
        private FtpConnection ftp;

        public IndexModule(IGithubUserRepository githubUserRepository, IDeploymentRepository deploymentRepository, IRootPathProvider rootPathProvider)
        {
            this.githubUserRepository = githubUserRepository;
            this.rootPathProvider = rootPathProvider;

            this.snowPreCompilerPath = rootPathProvider.GetRootPath() + "PreCompiler\\Sandra.Snow.Precompiler.exe";

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

                    var request = new RestRequest("users/" + githubUser + "/repos");
                    request.AddHeader("Accept", "application/json");

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
            var token = githubUserRepository.GetToken(username);
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
            if (model.AzureDeployment)
            {
                var remoteProcess =
                     Process.Start("\"" + gitLocation + "\"", " --git-dir=\"" + fullRepoPath + "\" remote add blog " + model.AzureRepo);
                if (remoteProcess != null)
                    remoteProcess.WaitForExit();

                var pushProcess = Process.Start("\"" + gitLocation + "\"", " --git-dir=\"" + fullRepoPath + "\" push -f blog master");
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