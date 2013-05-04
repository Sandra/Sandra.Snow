using System.Diagnostics;
using Nancy.ModelBinding;
using Sandra.Snow.Barbato.Model;
using LibGit2Sharp;
using Repository = LibGit2Sharp.Repository;

namespace Sandra.Snow.Barbato
{
    using Nancy;

    public class IndexModule : NancyModule
    {
        public IndexModule(IRootPathProvider rootPathProvider)
        {
            Post["/"] = parameters =>
                {
                    var payloadModel = this.Bind<RootObject>();
                        
                    var repoPath = Repository.Discover(rootPathProvider.GetRootPath());
                    if (string.IsNullOrWhiteSpace(repoPath))
                    {
                        Repository.Clone(payloadModel.repository.url + ".git", repoPath);
                    }
                    else
                    {
                        var repo = new Repository(repoPath);
                        //Shell out to git.exe as LibGit2Sharp doesnt support Merge yet
                        var pullProcess = Process.Start("C:\\Program Files (x86)\\Git\bin\\git.exe --git-dir=\"" + repoPath + ".git\" pull upstream master");
                        if (pullProcess != null)
                            pullProcess.WaitForExit();
                    }

                    //Run the PreCompiler

                    var addProcess = Process.Start("C:\\Program Files (x86)\\Git\bin\\git.exe --git-dir=\"" + repoPath + ".git\" add -A");
                    if (addProcess != null)
                        addProcess.WaitForExit();

                    var commitProcess = Process.Start("C:\\Program Files (x86)\\Git\bin\\git.exe --git-dir=\"" + repoPath + ".git\" commit -a -m \"Static Content Regenerated\"");
                    if (commitProcess != null)
                        commitProcess.WaitForExit();

                    var pushProcess = Process.Start("C:\\Program Files (x86)\\Git\bin\\git.exe --git-dir=\"" + repoPath + ".git\" push upstream master");
                    if (pushProcess != null)
                        pushProcess.WaitForExit();

                    return 200;
                };
        }
    }
}