#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Logging.Loggers;
using LBPUnion.ProjectLighthouse.Types.Entities.Maintenance;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Maintenance;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance;

public static class MaintenanceHelper
{
    static MaintenanceHelper()
    {
        Commands = GetListOfInterfaceObjects<ICommand>();
        MaintenanceJobs = GetListOfInterfaceObjects<IMaintenanceJob>();
        MigrationTasks = GetListOfInterfaceObjects<MigrationTask>();
        RepeatingTasks = GetListOfInterfaceObjects<IRepeatingTask>();
    }
    
    public static List<ICommand> Commands { get; }
    public static List<IMaintenanceJob> MaintenanceJobs { get; }
    public static List<MigrationTask> MigrationTasks { get; }
    public static List<IRepeatingTask> RepeatingTasks { get; }

    public static async Task<List<LogLine>> RunCommand(IServiceProvider provider, string[] args)
    {
        if (args.Length < 1)
            throw new Exception("This should never happen. " +
                                "If it did, its because you tried to run a command before validating that the user actually wants to run one.");

        string baseCmd = args[0];
        args = args.Skip(1).ToArray();
        
        // Setup memory logger for output
        Logger logger = new();
        InMemoryLogger memoryLogger = new();
        logger.AddLogger(memoryLogger);

        ICommand? command = Commands
            .Where(command =>
                command.Aliases().Any(a => string.Equals(a, baseCmd, StringComparison.CurrentCultureIgnoreCase)))
            .FirstOrDefault(command => args.Length >= command.RequiredArgs());
        if (command == null)
        {
            logger.LogError("Failed to find command", LogArea.Command);
            logger.Flush();
            return memoryLogger.Lines;
        }
        try
        {
            logger.LogInfo("Running command " + command.Name(), LogArea.Command);

            await command.Run(provider, args, logger);

            logger.Flush();
            return memoryLogger.Lines;
        }
        catch(Exception e)
        {
            logger.LogError($"Failed to run command: {e.Message}", LogArea.Command);
            logger.LogError(e.ToDetailedException(), LogArea.Command);
            logger.Flush();
            return memoryLogger.Lines;
        }
    }

    public static async Task RunMaintenanceJob(string jobName)
    {
        IMaintenanceJob? job = MaintenanceJobs.FirstOrDefault(j => j.GetType().Name == jobName);
        if (job == null) throw new ArgumentNullException(nameof(jobName));

        await job.Run();
    }

    public static async Task<bool> RunMigration(DatabaseContext database, MigrationTask migrationTask)
    {
        // Migrations should never be run twice.
        Debug.Assert(!await database.CompletedMigrations.Has(m => m.MigrationName == migrationTask.GetType().Name),
            $"Tried to run migration {migrationTask.GetType().Name} twice");
        
        Logger.Info($"Running LH migration task {migrationTask.Name()}", LogArea.Database);
        
        bool success;
        Exception? exception = null;

        Stopwatch stopwatch = Stopwatch.StartNew();
        
        try
        {
            success = await migrationTask.Run(database);
        }
        catch(Exception e)
        {
            success = false;
            exception = e;
        }
        
        if (!success)
        {
            Logger.Error($"Could not run LH migration {migrationTask.Name()}", LogArea.Database);
            if (exception != null) Logger.Error(exception.ToDetailedException(), LogArea.Database);
            
            return false;
        }
        stopwatch.Stop();

        Logger.Success($"Successfully completed LH migration {migrationTask.Name()} in {stopwatch.ElapsedMilliseconds}ms", LogArea.Database);

        CompletedMigrationEntity completedMigration = new()
        {
            MigrationName = migrationTask.GetType().Name,
            RanAt = DateTime.UtcNow,
        };

        database.CompletedMigrations.Add(completedMigration);
        await database.SaveChangesAsync();
        return true;
    }

    private static List<T> GetListOfInterfaceObjects<T>() where T : class
    {
        return Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => (t.IsSubclassOf(typeof(T)) || t.GetInterfaces().Contains(typeof(T))) && t.GetConstructor(Type.EmptyTypes) != null)
            .Select(t => Activator.CreateInstance(t) as T)
            .ToList()!;
    }
}