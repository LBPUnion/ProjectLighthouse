using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LBPUnion.ProjectLighthouse.Administration;
using LBPUnion.ProjectLighthouse.Administration.Maintenance;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Files;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Logging.Loggers;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
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

        ServerStatics.ServerType = serverType;

        // Setup logging
        Logger.Instance.AddLogger(new ConsoleLogger());
        Logger.Instance.AddLogger(new FileLogger());

        Logger.Info($"Welcome to the Project Lighthouse {serverType.ToString()}!", LogArea.Startup);
        Logger.Info($"You are running version {VersionHelper.FullVersion}", LogArea.Startup);

        // Referencing ServerConfiguration.Instance here loads the config, see ServerConfiguration.cs for more information
        Logger.Success("Loaded config file version " + ServerConfiguration.Instance.ConfigVersion, LogArea.Startup);

        Logger.Info("Connecting to the database...", LogArea.Startup);
        bool dbConnected = ServerStatics.DbConnected;
        if (!dbConnected)
        {
            Logger.Error("Database unavailable! Exiting.", LogArea.Startup);
        }
        else
        {
            Logger.Success("Connected to the database!", LogArea.Startup);
        }

        if (!dbConnected) Environment.Exit(1);
        using Database database = new();
        
        #if !DEBUG
        if (serverType == ServerType.GameServer)
        #endif
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
        
        Logger.Info("Initializing repeating tasks...", LogArea.Startup);
        RepeatingTaskHandler.Initialize();

            // Create admin user if no users exist
        if (serverType == ServerType.Website && database.Users.CountAsync().Result == 0)
        {
            const string passwordClear = "lighthouse";
            string password = CryptoHelper.BCryptHash(CryptoHelper.Sha256Hash(passwordClear));
            
            User admin = database.CreateUser("admin", password).Result;
            admin.PermissionLevel = PermissionLevel.Administrator;
            admin.PasswordResetRequired = true;

            database.SaveChanges();

            Logger.Success("No users were found, so an admin user was created. " + 
                           $"The username is 'admin' and the password is '{passwordClear}'.", LogArea.Startup);
        }

        stopwatch.Stop();
        Logger.Success($"Ready! Startup took {stopwatch.ElapsedMilliseconds}ms. Passing off control to ASP.NET...", LogArea.Startup);
    }

    private static void migrateDatabase(Database database)
    {
        Logger.Info("Migrating database...", LogArea.Database);
        Stopwatch totalStopwatch = new();
        Stopwatch stopwatch = new();
        totalStopwatch.Start();
        stopwatch.Start();

        database.Database.MigrateAsync().Wait();
        stopwatch.Stop();
        Logger.Success($"Structure migration took {stopwatch.ElapsedMilliseconds}ms.", LogArea.Database);
        
        stopwatch.Reset();
        stopwatch.Start();

        List<CompletedMigration> completedMigrations = database.CompletedMigrations.ToList();
        List<IMigrationTask> migrationsToRun = MaintenanceHelper.MigrationTasks
            .Where(migrationTask => !completedMigrations
                .Select(m => m.MigrationName)
                .Contains(migrationTask.GetType().Name)
            ).ToList();
        
        foreach (IMigrationTask migrationTask in migrationsToRun)
        {
            MaintenanceHelper.RunMigration(migrationTask, database).Wait();
        }

        stopwatch.Stop();
        totalStopwatch.Stop();
        Logger.Success($"Extra migration tasks took {stopwatch.ElapsedMilliseconds}ms.", LogArea.Database);
        Logger.Success($"Total migration took {totalStopwatch.ElapsedMilliseconds}ms.", LogArea.Database);
    }
}