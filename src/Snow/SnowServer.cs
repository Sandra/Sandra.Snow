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

            var options = new StartOptions
            {
                ServerFactory = "Nowin",
                Port = 5498
            };

            Startup.Settings = settings;

            using (WebApp.Start<Startup>(options))
            {
                Console.WriteLine("Sandra.Snow : Listening on http://locahost:5498/");
                Console.WriteLine(" - attempting to open your browser...");
                
                Process.Start("http://localhost:5498/");

                Console.WriteLine(" - press any to quit the testing server...");
                Console.ReadKey();
                Console.WriteLine("");
                Console.WriteLine("Sandra.Snow : Exited testing server");
            }
        }

        public class Startup
        {
            public static SnowSettings Settings { get; set; }

            public void Configuration(IAppBuilder app)
            {
                var fileSystem = new FileServerOptions
                {
                    EnableDirectoryBrowsing = true,
                    FileSystem = new PhysicalFileSystem(Path.GetFullPath(Settings.Output))
                };

                app.UseFileServer(fileSystem);
            }
        }
    }
}