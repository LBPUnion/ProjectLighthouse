using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using LBPUnion.ProjectLighthouse.Administration.Maintenance;
using LBPUnion.ProjectLighthouse.Administration.Maintenance.MigrationTasks;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Files;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Logging.Loggers;
using LBPUnion.ProjectLighthouse.StorableLists;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Maintenance;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Maintenance;
using LBPUnion.ProjectLighthouse.Types.Users;
using Medallion.Threading.MySql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using ServerType = LBPUnion.ProjectLighthouse.Types.Misc.ServerType;

namespace LBPUnion.ProjectLighthouse;

public static class StartupTasks
{

    private class OldScoreEntity
    {
        public int ScoreId { get; set; }

        public int SlotId { get; set; }

        public int ChildSlotId { get; set; }

        public int Type { get; set; }

        public string PlayerIdCollection { get; set; }

        [NotMapped]
        public string[] PlayerIds
        {
            get => this.PlayerIdCollection.Split(",");
            set => this.PlayerIdCollection = string.Join(',', value);
        }

        public int UserId { get; set; }

        public int Points { get; set; }

        public long Timestamp { get; set; }
    }
    public static async Task Run(ServerType serverType)
    {
        // Log startup time
        Stopwatch stopwatch = new();
        stopwatch.Start();

        ServerStatics.ServerType = serverType;

        // Setup logging
        Logger.Instance.AddLogger(new ConsoleLogger());
        Logger.Instance.AddLogger(new FileLogger());

        Logger.Info($"Welcome to the Project Lighthouse {serverType.ToString()}!", LogArea.Startup);

        Logger.Info("Loading configurations...", LogArea.Startup);
        if (!LoadConfigurations())
        {
            Logger.Error("Failed to load one or more configurations", LogArea.Config);
            Environment.Exit(1);
        }

        // Version info depends on ServerConfig 
        Logger.Info($"You are running version {VersionHelper.FullVersion}", LogArea.Startup);
        Logger.Info("Connecting to the database...", LogArea.Startup);

        await using DatabaseContext database = DatabaseContext.CreateNewInstance();
        try
        {
            if (!await database.Database.CanConnectAsync())
            {
                Logger.Error("Database unavailable! Exiting.", LogArea.Startup);
                Logger.Error("Ensure that you have set the dbConnectionString field in lighthouse.yml", LogArea.Startup);
                Environment.Exit(-1);
            }
        }
        catch (Exception e)
        {
            Logger.Error("There was an error connecting to the database:", LogArea.Startup);
            Logger.Error(e.ToDetailedException(), LogArea.Startup);
            Environment.Exit(-1);
        }

        await MigrateDatabase(database);

        Logger.Debug
        (
            "This is a debug build, so performance may suffer! " +
            "If you are running Lighthouse in a production environment, " +
            "it is highly recommended to run a release build. ",
            LogArea.Startup
        );
        Logger.Debug("You can do so by running any dotnet command with the flag: \"-c Release\". ", LogArea.Startup);

        if (ServerConfiguration.Instance.WebsiteConfiguration.ConvertAssetsOnStartup
            && serverType == ServerType.Website)
        {
            FileHelper.ConvertAllTexturesToPng();
        }

        Logger.Info("Initializing Redis...", LogArea.Startup);
        RedisDatabase.Initialize().Wait();

        // Create admin user if no users exist
        if (serverType == ServerType.Website && database.Users.CountAsync().Result == 0)
        {
            const string passwordClear = "lighthouse";
            string password = CryptoHelper.BCryptHash(CryptoHelper.Sha256Hash(passwordClear));
            
            UserEntity admin = database.CreateUser("admin", password).Result;
            admin.PermissionLevel = PermissionLevel.Administrator;
            admin.PasswordResetRequired = true;

            await database.SaveChangesAsync();

            Logger.Success("No users were found, so an admin user was created. " + 
                           $"The username is 'admin' and the password is '{passwordClear}'.", LogArea.Startup);
        }

        stopwatch.Stop();
        Logger.Success($"Ready! Startup took {stopwatch.ElapsedMilliseconds}ms. Passing off control to ASP.NET...", LogArea.Startup);
    }

    private static bool LoadConfigurations()
    {
        Assembly assembly = Assembly.GetAssembly(typeof(ConfigurationBase<>));
        if (assembly == null) return false;
        bool didLoad = true;
        foreach (Type type in assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.BaseType?.Name == "ConfigurationBase`1"))
        {
            if (type.BaseType == null) continue;
            if (type.BaseType.GetProperty("Instance") != null)
            {
                // force create lazy instance
                type.BaseType.GetProperty("Instance")?.GetValue(null);
                bool isConfigured = false;
                while (!isConfigured)
                {
                    isConfigured = (bool)(type.BaseType.GetProperty("IsConfigured")?.GetValue(null) ?? false);
                    Thread.Sleep(10);
                }
            }

            object objRef = type.BaseType.GetProperty("Instance")?.GetValue(null);
            int configVersion = ((int?)type.GetProperty("ConfigVersion")?.GetValue(objRef)).GetValueOrDefault();
            if (configVersion <= 0)
            {
                didLoad = false;
            }
            else
            {
                Logger.Success($"Successfully loaded {type.Name} version {configVersion}", LogArea.Startup);
            }
        }

        return didLoad;
    }

    private static async Task MigrateDatabase(DatabaseContext database)
    {
        int? originalTimeout = database.Database.GetCommandTimeout();
        database.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));
        // This mutex is used to synchronize migrations across the GameServer, Website, and Api
        // Without it, each server would try to simultaneously migrate the database resulting in undefined behavior
        // It is only used for startup and immediately disposed after migrating
        Stopwatch totalStopwatch = Stopwatch.StartNew();
        Stopwatch stopwatch = Stopwatch.StartNew();
        Logger.Info("Migrating database...", LogArea.Database);
        MySqlDistributedLock mutex = new("LighthouseMigration", ServerConfiguration.Instance.DbConnectionString);
        await using (await mutex.AcquireAsync())
        {
            stopwatch.Stop();
            Logger.Success($"Acquiring migration lock took {stopwatch.ElapsedMilliseconds}ms", LogArea.Database);

            stopwatch.Restart();
            List<string> pendingMigrations = (await database.Database.GetPendingMigrationsAsync()).ToList();
            IMigrator migrator = database.GetInfrastructure().GetService<IMigrator>();

            async Task RunLighthouseMigrations(Func<MigrationTask, bool> predicate)
            {
                List<MigrationTask> tasks = MaintenanceHelper.MigrationTasks
                    .Where(predicate)
                    .ToList();
                foreach (MigrationTask task in tasks)
                {
                    await MaintenanceHelper.RunMigration(database, task);
                }
            }

            Logger.Info($"There are {pendingMigrations.Count} pending migrations", LogArea.Database);
            foreach (string migration in pendingMigrations)
            {
                try
                {
                    stopwatch.Restart();
                    await using IDbContextTransaction transaction = await database.Database.BeginTransactionAsync();
                    await RunLighthouseMigrations(m => m.Name() == migration && m.HookType() == MigrationHook.Before);
                    await migrator.MigrateAsync(migration);
                    await RunLighthouseMigrations(m => m.Name() == migration && m.HookType() == MigrationHook.After);
                    await transaction.CommitAsync();
                    stopwatch.Stop();
                    Logger.Success($"Running migration '{migration}' took {stopwatch.ElapsedMilliseconds}ms.", LogArea.Database);
                    
                }
                catch (Exception e)
                {
                    await database.Database.RollbackTransactionAsync();
                    Logger.Error($"Failed to run migration '{migration}'", LogArea.Database);
                    Logger.Error(e.ToDetailedException(), LogArea.Database);
                    Environment.Exit(-1);
                }
            }

            stopwatch.Restart();

            List<string> completedMigrations = database.CompletedMigrations.Select(m => m.MigrationName).ToList();

            await RunLighthouseMigrations(m => m.HookType() == MigrationHook.None && !completedMigrations.Contains(m.Name()));

            stopwatch.Stop();
            totalStopwatch.Stop();
            Logger.Success($"Extra migration tasks took {stopwatch.ElapsedMilliseconds}ms.", LogArea.Database);
            Logger.Success($"Total migration took {totalStopwatch.ElapsedMilliseconds}ms.", LogArea.Database);
        }
        database.Database.SetCommandTimeout(originalTimeout);
    }
}