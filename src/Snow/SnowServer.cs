namespace Snow
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using Extensions;

    public class SnowServer
    {
        public void Start(SnowSettings settings)
        {
            Directory.SetCurrentDirectory(settings.Output);

            using (var server = new HttpListener())
            {
                server.Prefixes.Add("http://localhost:1234/");
                server.Start();

                "Listening on http://localhost:1234/".OutputIfDebug();

                while (true)
                {
                    var context = server.GetContext();
                    var response = context.Response;

                    var requestUrl = Directory.GetCurrentDirectory() + context.Request.Url.LocalPath;

                    if (context.Request.Url.LocalPath == "/")
                    {
                        requestUrl = Directory.GetCurrentDirectory() + "/index.html";
                    }

                    using (var tr = new FileStream(requestUrl, FileMode.Open))
                    using (var output = response.OutputStream)
                    {
                       tr.CopyTo(output);
                        output.Flush();
                        //var msg = tr.ReadToEnd();

                        //var buffer = Encoding.UTF8.GetBytes(msg);

                        //response.ContentLength64 = buffer.Length;
                        //var st = response.OutputStream;
                        //st.Write(buffer, 0, buffer.Length);
                        //st.Flush();
                    }

                    context.Response.Close();  // here we close the connection
                }
            }
        }
    }
}