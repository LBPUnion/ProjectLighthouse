#nullable enable
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Kettu;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Files;
using LBPUnion.ProjectLighthouse.Types.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LBPUnion.ProjectLighthouse;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length != 0 && args[0] == "--wait-for-debugger")
        {
            Console.WriteLine("Waiting for a debugger to be attached...");
            while (!Debugger.IsAttached) Thread.Sleep(100);
            Console.WriteLine("Debugger attached.");
        }

        // Log startup time
        Stopwatch stopwatch = new();
        stopwatch.Start();

        // Setup logging

        Logger.StartLogging();
        Logger.UpdateRate /= 2;
        LoggerLine.LogFormat = "[{0}] {1}";
        Logger.AddLogger(new ConsoleLogger());
        Logger.AddLogger(new LighthouseFileLogger());

        Logger.Log("Welcome to Project Lighthouse!", LoggerLevelStartup.Instance);
        Logger.Log($"Running {VersionHelper.FullVersion}", LoggerLevelStartup.Instance);

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
            InfluxHelper.StartLogging().Wait();
            if (ServerSettings.Instance.InfluxLoggingEnabled) Logger.AddLogger(new InfluxLogger());
        }

        #if DEBUG
        Logger.Log
        (
            "This is a debug build, so performance may suffer! " +
            "If you are running Lighthouse in a production environment, " +
            "it is highly recommended to run a release build. ",
            LoggerLevelStartup.Instance
        );
        Logger.Log("You can do so by running any dotnet command with the flag: \"-c Release\". ", LoggerLevelStartup.Instance);
        #endif

        if (args.Length != 0)
        {
            MaintenanceHelper.RunCommand(args).Wait();
            return;
        }

        if (Directory.Exists("r"))
        {
            Logger.Log
                ("Converting all textures to PNG. This may take a while if this is the first time running this operation...", LoggerLevelStartup.Instance);

            ConcurrentQueue<string> fileQueue = new();

            foreach (string filename in Directory.GetFiles("r")) fileQueue.Enqueue(filename);

            for(int i = 0; i < Environment.ProcessorCount; i++)
            {
                Task.Factory.StartNew
                (
                    () =>
                    {
                        while (fileQueue.TryDequeue(out string? filename))
                        {
                            LbpFile? file = LbpFile.FromHash(filename.Replace("r/", ""));
                            if (file == null) continue;

                            if (file.FileType == LbpFileType.Jpeg || file.FileType == LbpFileType.Png || file.FileType == LbpFileType.Texture)
                            {
                                ImageHelper.LbpFileToPNG(file);
                            }
                        }
                    }
                );
            }

            while (!fileQueue.IsEmpty)
            {
                Thread.Sleep(100);
            }
        }

        stopwatch.Stop();
        Logger.Log($"Ready! Startup took {stopwatch.ElapsedMilliseconds}ms. Passing off control to ASP.NET...", LoggerLevelStartup.Instance);

        CreateHostBuilder(args).Build().Run();
    }

    public static void MigrateDatabase(Database database)
    {
        Stopwatch stopwatch = new();
        stopwatch.Start();

        database.Database.MigrateAsync().Wait();

        stopwatch.Stop();
        Logger.Log($"Migration took {stopwatch.ElapsedMilliseconds}ms.", LoggerLevelDatabase.Instance);
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
        => Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults
            (
                webBuilder =>
                {
                    webBuilder.UseStartup<Startup.Startup>();
                    webBuilder.UseWebRoot("StaticFiles");
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