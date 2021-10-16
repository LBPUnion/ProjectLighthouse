using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProjectLighthouse.Types;

namespace ProjectLighthouse {
    public static class Program {
        public static void Main(string[] args) {
            Console.WriteLine("Welcome to Project Lighthouse!");
            Console.WriteLine(ServerSettings.DbConnected ? "Connected to the database." : "Database unavailable. Starting in stateless mode.");

            IHostBuilder builder = Host.CreateDefaultBuilder(args);
            
            builder.ConfigureWebHostDefaults(webBuilder => {
                webBuilder.UseStartup<Startup>();
            });

            builder.Build().Run();
        }
    }
}