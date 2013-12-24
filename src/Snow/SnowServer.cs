namespace Snow
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using Microsoft.Owin.FileSystems;
    using Microsoft.Owin.Hosting;
    using Microsoft.Owin.StaticFiles;
    using Owin;

    public class SnowServer
    {
        public static void Start(SnowSettings settings)
        {
            Console.WriteLine("Sandra.Snow : Starting up a testing server");
            
            var fileSystem = new FileServerOptions
            {
                EnableDirectoryBrowsing = true,
                FileSystem = new PhysicalFileSystem(Path.GetFullPath(settings.Output))
            };

            using (WebApp.Start("http://*:1234/", app => app.UseFileServer(fileSystem)))
            {
                Console.WriteLine("Sandra.Snow : Listening on http://locahost:1234/");
                Console.WriteLine(" - attempting to open your browser...");
                
                Process.Start("http://localhost:1234/");

                Console.WriteLine(" - press any to quit the testing server...");
                Console.ReadKey();
                Console.WriteLine("");
                Console.WriteLine("Sandra.Snow : Exited testing server");
            }
        }
    }
}