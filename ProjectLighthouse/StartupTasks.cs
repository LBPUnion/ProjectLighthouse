using System;
using System.Collections.Generic;
using System.Diagnostics;
using LBPUnion.ProjectLighthouse.Administration.Maintenance;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Files;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Logging.Loggers;
using LBPUnion.ProjectLighthouse.Match.Rooms;
using LBPUnion.ProjectLighthouse.Startup;
using LBPUnion.ProjectLighthouse.StorableLists;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse;

public static class StartupTasks
{
    public static void Run(string[] args, ServerType serverType)
    {
        // Log startup time
        Stopwatch stopwatch = new();
        stopwatch.Start();

        #if DEBUG
        DebugWarmupLifetime.ServerType = serverType;
        #endif

        // Setup logging
        Logger.Instance.AddLogger(new ConsoleLogger());
        Logger.Instance.AddLogger(new LighthouseFileLogger());

        Logger.Info($"Welcome to the Project Lighthouse {serverType.ToString()}!", LogArea.Startup);
        Logger.Info($"You are running version {VersionHelper.FullVersion}", LogArea.Startup);

        // Referencing ServerSettings.Instance here loads the config, see ServerSettings.cs for more information
        Logger.Success("Loaded config file version " + ServerConfiguration.Instance.ConfigVersion, LogArea.Startup);

        Logger.Info("Connecting to the database...", LogArea.Startup);
        bool dbConnected = ServerStatics.DbConnected;
        if (!dbConnected)
        {
            Logger.Error("Database unavailable! Exiting.", LogArea.Startup);
        }
        else
        {
            Logger.Success("Connected!", LogArea.Startup);
        }

        if (!dbConnected) Environment.Exit(1);
        using Database database = new();

        Logger.Info("Migrating database...", LogArea.Database);
        migrateDatabase(database);
        
        if (ServerConfiguration.Instance.InfluxDB.InfluxEnabled)
        {
            Logger.Info("Influx logging is enabled. Starting influx logging...", LogArea.Startup);
            InfluxHelper.StartLogging().Wait();
            if (ServerConfiguration.Instance.InfluxDB.LoggingEnabled) Logger.Instance.AddLogger(new InfluxLogger());
        }

        Logger.Debug
        (
            "This is a debug build, so performance may suffer! " +
            "If you are running Lighthouse in a production environment, " +
            "it is highly recommended to run a release build. ",
            LogArea.Startup
        );
        Logger.Debug("You can do so by running any dotnet command with the flag: \"-c Release\". ", LogArea.Startup);

        if (args.Length != 0)
        {
            List<LogLine> logLines = MaintenanceHelper.RunCommand(args).Result;
            Console.WriteLine(logLines.ToLogString());
            return;
        }

        if (ServerConfiguration.Instance.WebsiteConfiguration.ConvertAssetsOnStartup
            && serverType == ServerType.Website)
        {
            FileHelper.ConvertAllTexturesToPng();
        }
        
        Logger.Info("Initializing Redis...", LogArea.Startup);
        RedisDatabase.Initialize().Wait();

        if (serverType == ServerType.GameServer)
        {
            Logger.Info("Starting room cleanup thread...", LogArea.Startup);
            RoomHelper.StartCleanupThread();
        }

        stopwatch.Stop();
        Logger.Success($"Ready! Startup took {stopwatch.ElapsedMilliseconds}ms. Passing off control to ASP.NET...", LogArea.Startup);
    }

    private static void migrateDatabase(Database database)
    {
        Stopwatch stopwatch = new();
        stopwatch.Start();

        database.Database.MigrateAsync().Wait();

        stopwatch.Stop();
        Logger.Success($"Migration took {stopwatch.ElapsedMilliseconds}ms.", LogArea.Database);
    }
}