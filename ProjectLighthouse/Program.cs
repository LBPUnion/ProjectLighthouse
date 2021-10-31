using System;
using System.Diagnostics;
using System.Linq;
using Kettu;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Profiles;
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
            Logger.Log("Determining if the database is available...", LoggerLevelStartup.Instance);
            bool dbConnected = ServerSettings.DbConnected;
            Logger.Log(dbConnected ? "Connected to the database." : "Database unavailable! Exiting.", LoggerLevelStartup.Instance);

            if (!dbConnected) Environment.Exit(1);
            using Database database = new();

            Logger.Log("Migrating database...", LoggerLevelDatabase.Instance);
            MigrateDatabase(database);

            Logger.Log("Fixing broken timestamps...", LoggerLevelDatabase.Instance);
            FixTimestamps(database);

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

        public static void FixTimestamps(Database database)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();

            foreach (Slot slot in database.Slots.Where(s => s.FirstUploaded == 0)) slot.FirstUploaded = TimeHelper.UnixTimeMilliseconds();

            foreach (Slot slot in database.Slots.Where(s => s.LastUpdated == 0)) slot.LastUpdated = TimeHelper.UnixTimeMilliseconds();

            foreach (Comment comment in database.Comments.Where(c => c.Timestamp == 0)) comment.Timestamp = TimeHelper.UnixTimeMilliseconds();

            database.SaveChanges();

            stopwatch.Stop();
            Logger.Log($"Fixing timestamps took {stopwatch.ElapsedMilliseconds}ms.", LoggerLevelDatabase.Instance);
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