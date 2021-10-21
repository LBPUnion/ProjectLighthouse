using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using ProjectLighthouse.Types.Settings;

namespace ProjectLighthouse {
    public static class Program {
        public static void Main(string[] args) {
            Stopwatch startupStopwatch = new();
            startupStopwatch.Start();
            Console.WriteLine("Welcome to Project Lighthouse!");
            Console.WriteLine("Determining if the database is available...");
            bool dbConnected = ServerSettings.DbConnected;
            Console.WriteLine(dbConnected ? "Connected to the database." : "Database unavailable! Exiting.");

            if(dbConnected) {
                Stopwatch migrationStopwatch = new();
                migrationStopwatch.Start();
                
                Console.WriteLine("Migrating database...");
                using Database database = new();
                database.Database.Migrate();
                
                migrationStopwatch.Stop();
                Console.WriteLine($"Migration took {migrationStopwatch.ElapsedMilliseconds}ms.");
            } else Environment.Exit(1);
            
            startupStopwatch.Stop();
            Console.WriteLine($"Ready! Startup took {startupStopwatch.ElapsedMilliseconds}ms. Passing off control to ASP.NET...");

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                });
    }
}