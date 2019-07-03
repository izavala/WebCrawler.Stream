using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Owin.StaticFiles;
using Owin;

namespace WebCrawler.Stream
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseFileServer(new FileServerOptions
            {
                EnableDirectoryBrowsing = true
            });
            app.MapSignalR<EchoConnection>("/echo");
        }
    }
}
