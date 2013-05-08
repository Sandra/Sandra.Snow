namespace Sandra.Snow.Barbato
{
    using Nancy;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using Nancy.ModelBinding;
    using Sandra.Snow.Barbato.Model;

    public class IndexModule : NancyModule
    {
        public IndexModule(IRootPathProvider rootPathProvider, IUserRepository userRepository)
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
        }
    }
}