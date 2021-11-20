using System;
using System.Diagnostics;
using Kettu;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LBPUnion.ProjectLighthouse
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // Log startup time
            Stopwatch stopwatch = new();
            stopwatch.Start();

            // Setup logging

            Logger.StartLogging();
            LoggerLine.LogFormat = "[{0}] {1}";
            Logger.AddLogger(new ConsoleLogger());
            Logger.AddLogger(new LighthouseFileLogger());

            Logger.Log("Welcome to Project Lighthouse!", LoggerLevelStartup.Instance);
            Logger.Log($"Running {GitVersionHelper.FullVersion}", LoggerLevelStartup.Instance);

            // This loads the config, see ServerSettings.cs for more information
            Logger.Log("Loaded config file version " + ServerSettings.Instance.ConfigVersion, LoggerLevelStartup.Instance);

            Logger.Log("Determining if the database is available...", LoggerLevelStartup.Instance);
            bool dbConnected = ServerStatics.DbConnected;
            Logger.Log(dbConnected ? "Connected to the database." : "Database unavailable! Exiting.", LoggerLevelStartup.Instance);

            if (!dbConnected) Environment.Exit(1);
            using Database database = new();

            Logger.Log("Migrating database...", LoggerLevelDatabase.Instance);
            MigrateDatabase(database);

            if (ServerSettings.Instance.InfluxEnabled)
            {
                Logger.Log("Influx logging is enabled. Starting influx logging...", LoggerLevelStartup.Instance);
                #pragma warning disable CS4014
                InfluxHelper.StartLogging();
                #pragma warning restore CS4014
                if (ServerSettings.Instance.InfluxLoggingEnabled) Logger.AddLogger(new InfluxLogger());
            }

            stopwatch.Stop();
            Logger.Log($"Ready! Startup took {stopwatch.ElapsedMilliseconds}ms. Passing off control to ASP.NET...", LoggerLevelStartup.Instance);

            CreateHostBuilder(args).Build().Run();
        }

        public static void MigrateDatabase(Database database)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();

            database.Database.Migrate();

            stopwatch.Stop();
            Logger.Log($"Migration took {stopwatch.ElapsedMilliseconds}ms.", LoggerLevelDatabase.Instance);
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults
                (
                    webBuilder =>
                    {
                        webBuilder.UseStartup<Startup>();
                    }
                )
                .ConfigureLogging
                (
                    logging =>
                    {
                        logging.ClearProviders();
                        logging.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, AspNetToKettuLoggerProvider>());
                    }
                );
    }
}