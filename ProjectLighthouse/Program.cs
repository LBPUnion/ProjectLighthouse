using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using ProjectLighthouse.Types;

namespace ProjectLighthouse {
    public static class Program {
        public static void Main(string[] args) {
            Console.WriteLine("Welcome to Project Lighthouse!");
            bool dbConnected = ServerSettings.DbConnected;
            Console.WriteLine(dbConnected ? "Connected to the database." : "Database unavailable. Exiting.");

            if(dbConnected) {
                Console.WriteLine("Migrating database...");
                new Database().Database.Migrate();
            }
            else {
                Environment.Exit(1);
            }

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                });
    }
}