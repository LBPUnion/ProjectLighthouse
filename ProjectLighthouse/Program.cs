using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProjectLighthouse.Types;

namespace ProjectLighthouse {
    public static class Program {
        public static void Main(string[] args) {
            Console.WriteLine("Welcome to Project Lighthouse!");
            Console.WriteLine(ServerSettings.DbConnected ? "Connected to the database." : "Database unavailable. Starting in stateless mode.");

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                });
    }
}