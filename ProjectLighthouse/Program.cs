using System;
using System.Diagnostics;
using Kettu;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace LBPUnion.ProjectLighthouse {
    public static class Program {
        public static void Main(string[] args) {
            // Log startup time
            Stopwatch startupStopwatch = new();
            startupStopwatch.Start();
            
            // Setup logging
            
            Logger.StartLogging();
            LoggerLine.LogFormat = "[{0}] {1}";
            Logger.AddLogger(new ConsoleLogger());
            Logger.AddLogger(new LighthouseFileLogger());
            
            Logger.Log("Welcome to Project Lighthouse!", LoggerLevelStartup.Instance);
            Logger.Log("Determining if the database is available...", LoggerLevelStartup.Instance);
            bool dbConnected = ServerSettings.DbConnected;
            Logger.Log(dbConnected ? "Connected to the database." : "Database unavailable! Exiting.", LoggerLevelStartup.Instance);

            if(dbConnected) {
                Stopwatch migrationStopwatch = new();
                migrationStopwatch.Start();
                
                Logger.Log("Migrating database...", LoggerLevelDatabase.Instance);
                using Database database = new();
                database.Database.Migrate();
                
                migrationStopwatch.Stop();
                Logger.Log($"Migration took {migrationStopwatch.ElapsedMilliseconds}ms.", LoggerLevelDatabase.Instance);
            } else Environment.Exit(1);
            
            startupStopwatch.Stop();
            Logger.Log($"Ready! Startup took {startupStopwatch.ElapsedMilliseconds}ms. Passing off control to ASP.NET...", LoggerLevelStartup.Instance);

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureLogging(logging => {
                    logging.ClearProviders();
                    logging.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, AspNetToKettuLoggerProvider>());
                });
    }
}