using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace PoliticalPurse.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            var portVar = Environment.GetEnvironmentVariable("PORT");
            if(!int.TryParse(portVar, out int port))
            {
                port = 5000;
            }

            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls("http://localhost:" + port)
                .Build();
        }
    }
}
