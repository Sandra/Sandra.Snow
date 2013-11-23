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
    using NLog;
    using Nancy;
    using Nancy.ModelBinding;
    using RestSharp;

    public class IndexModule : NancyModule
    {
        private readonly IGithubUserRepository githubUserRepository;
        private readonly string gitLocation = ConfigurationManager.AppSettings["GitLocation"];
        private readonly string snowPreCompilerPath;
        private string repoPath = ConfigurationManager.AppSettings["ClonedGitFolder"];
        private string fullRepoPath;
        private string fullPublishGitPath;
        private string publishGitPath;
        private FtpConnection ftp;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public IndexModule(IGithubUserRepository githubUserRepository, IDeploymentRepository deploymentRepository, IRootPathProvider rootPathProvider)
        {
            this.githubUserRepository = githubUserRepository;

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

            Logger.Debug("Deleting content from " + repoPath);
            DeleteRepoPathContents(repoPath);
            Directory.Delete(repoPath);
            Logger.Debug("Deleted files/folders");
        }

        private void CloneFromPublishLocation(DeploymentModel model)
        {
            Logger.Debug(model.GitDeployment ? "Git deployment" : "Non Git Deployment");

            publishGitPath = repoPath + "\\" + "Website";

            if (model.GitDeployment)
            {
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

            Logger.Debug("Trying to clone from Github with" + Environment.NewLine + "\turl : " + cloneUrl +
                         Environment.NewLine + "\tpath : " + repoPath);

            var process = new Process();
            var startInfo = new ProcessStartInfo(gitLocation)
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    Arguments = " clone " + cloneUrl + " " + repoPath
                };
            process.StartInfo = startInfo;
            process.OutputDataReceived += (sender, args) => Logger.Debug(args.Data);
            process.ErrorDataReceived += (sender, args) => Logger.Debug(args.Data);
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            Logger.Debug("Waiting for GH clone process to exit");
        }

        private void CloneFromPublishGitRepository(string repoUrl, string outputDir)
        {
            Logger.Debug("Trying to clone from deployment repo with" + Environment.NewLine + "\turl : " + repoUrl +
                         Environment.NewLine + "\tpath : " + outputDir);

            var cloneProcess =
               Process.Start(gitLocation, " clone " + repoUrl + " " + outputDir);

            Logger.Debug("Waiting for publish clone process to exit");

            if (cloneProcess != null)
                cloneProcess.WaitForExit();
        }

        private void LetItSnow()
        {
            Logger.Debug("Making it Snow");

            var snowProcess = Process.Start(snowPreCompilerPath, " config=" + repoPath + "\\");

            Logger.Debug("Waiting for Snow process to exit");

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
                Logger.Debug("Executing git add");

                var addProcess = new Process();
                var addProcessStartInfo = new ProcessStartInfo("\"" + gitLocation + "\"")
                    {
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        Arguments = " --git-dir=\"" + fullPublishGitPath + "\" --work-tree=\"" + publishGitPath + "\" add -A"

                    };
                addProcess.StartInfo = addProcessStartInfo;
                addProcess.OutputDataReceived += (sender, args) => Logger.Debug(args.Data);
                addProcess.ErrorDataReceived += (sender, args) => Logger.Debug(args.Data);
                addProcess.Start();
                addProcess.BeginOutputReadLine();
                addProcess.BeginErrorReadLine();
                addProcess.WaitForExit();

                Logger.Debug("git add process to exited");

                Logger.Debug("Executing git email config process");

                var emailProcess = new Process();
                var emailProcessStartInfo = new ProcessStartInfo("\"" + gitLocation + "\"")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    Arguments = " --git-dir=\"" + fullPublishGitPath + "\" --work-tree=\"" + publishGitPath + "\" config user.email \"barbato@azurewebsites.net\""

                };
                emailProcess.StartInfo = emailProcessStartInfo;
                emailProcess.OutputDataReceived += (sender, args) => Logger.Debug(args.Data);
                emailProcess.ErrorDataReceived += (sender, args) => Logger.Debug(args.Data);
                emailProcess.Start();
                emailProcess.BeginOutputReadLine();
                emailProcess.BeginErrorReadLine();
                emailProcess.WaitForExit();
                Logger.Debug("git email config process to exited");

                Logger.Debug("Executing git name config process");

                var userProcess = new Process();
                var userProcessStartInfo = new ProcessStartInfo("\"" + gitLocation + "\"")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    Arguments = " --git-dir=\"" + fullPublishGitPath + "\" --work-tree=\"" + publishGitPath + "\" config user.name \"barbato\""

                };
                userProcess.StartInfo = userProcessStartInfo;
                userProcess.OutputDataReceived += (sender, args) => Logger.Debug(args.Data);
                userProcess.ErrorDataReceived += (sender, args) => Logger.Debug(args.Data);
                userProcess.Start();
                userProcess.BeginOutputReadLine();
                userProcess.BeginErrorReadLine();
                userProcess.WaitForExit();

                Logger.Debug("git name config process to exited");

                Logger.Debug("Executing git commit");

                var commitProcess = new Process();
                var commitProcessStartInfo = new ProcessStartInfo("\"" + gitLocation + "\"")
                    {
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        Arguments = " --git-dir=\"" + fullPublishGitPath + "\" --work-tree=\"" + publishGitPath +
                                    "\" commit -a -m \"Static Content Regenerated\""
                    };
                commitProcess.StartInfo = commitProcessStartInfo;
                commitProcess.OutputDataReceived += (sender, args) => Logger.Debug(args.Data);
                commitProcess.ErrorDataReceived += (sender, args) => Logger.Debug(args.Data);
                commitProcess.Start();
                commitProcess.BeginOutputReadLine();
                commitProcess.BeginErrorReadLine();
                commitProcess.WaitForExit();

                Logger.Debug("git commit process to exited");
               
                Logger.Debug("Executing git push");

                var pushProcess = new Process();
                var pushProcessStartInfo = new ProcessStartInfo("\"" + gitLocation + "\"")
                    {
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        Arguments = " --git-dir=\"" + fullPublishGitPath + "\" push -f origin master"
                    };
                pushProcess.OutputDataReceived += (sender, args) => Logger.Debug(args.Data);
                pushProcess.ErrorDataReceived += (sender, args) => Logger.Debug(args.Data);
                pushProcess.StartInfo = pushProcessStartInfo;
                pushProcess.Start();
                pushProcess.BeginOutputReadLine();
                pushProcess.BeginErrorReadLine();
                pushProcess.WaitForExit();

                Logger.Debug("git push process to exited");

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

                        FtpBlogFiles(publishGitPath, model.FTPPath);
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