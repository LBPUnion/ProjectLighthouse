#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Logging.Loggers;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance;

public static class MaintenanceHelper
{
    static MaintenanceHelper()
    {
        Commands = getListOfInterfaceObjects<ICommand>();
        MaintenanceJobs = getListOfInterfaceObjects<IMaintenanceJob>();
        MigrationTasks = getListOfInterfaceObjects<IMigrationTask>();
        RepeatingTasks = getListOfInterfaceObjects<IRepeatingTask>();
    }
    
    public static List<ICommand> Commands { get; }
    public static List<IMaintenanceJob> MaintenanceJobs { get; }
    public static List<IMigrationTask> MigrationTasks { get; }
    public static List<IRepeatingTask> RepeatingTasks { get; }

    public static async Task<List<LogLine>> RunCommand(string[] args)
    {
        if (args.Length < 1)
            throw new Exception
                ("This should never happen. " + "If it did, its because you tried to run a command before validating that the user actually wants to run one.");

        string baseCmd = args[0];
        args = args.Skip(1).ToArray();
        
        // Setup memory logger for output
        Logger logger = new();
        InMemoryLogger memoryLogger = new();
        logger.AddLogger(memoryLogger);

        IEnumerable<ICommand> suitableCommands = Commands.Where
                (command => command.Aliases().Any(a => a.ToLower() == baseCmd.ToLower()))
            .Where(command => args.Length >= command.RequiredArgs());
        foreach (ICommand command in suitableCommands)
        {
            logger.LogInfo("Running command " + command.Name(), LogArea.Command);
            
            await command.Run(args, logger);
            logger.Flush();
            return memoryLogger.Lines;
        }

        logger.LogError("Command not found.", LogArea.Command);
        logger.Flush();
        return memoryLogger.Lines;
    }

    public static async Task RunMaintenanceJob(string jobName)
    {
        IMaintenanceJob? job = MaintenanceJobs.FirstOrDefault(j => j.GetType().Name == jobName);
        if (job == null) throw new ArgumentNullException();

        await job.Run();
    }

    public static async Task RunMigration(IMigrationTask migrationTask, Database? database = null)
    {
        database ??= new Database();

        // Migrations should never be run twice.
        Debug.Assert(!await database.CompletedMigrations.Has(m => m.MigrationName == migrationTask.GetType().Name));
        
        Logger.Info($"Running migration task {migrationTask.Name()}", LogArea.Database);
        
        bool success;
        Exception? exception = null;
        
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
            Logger.Error($"Could not run migration {migrationTask.Name()}", LogArea.Database);
            if (exception != null) Logger.Error(exception.ToDetailedException(), LogArea.Database);
            
            return;
        }
        
        Logger.Success($"Successfully completed migration {migrationTask.Name()}", LogArea.Database);

        CompletedMigration completedMigration = new()
        {
            MigrationName = migrationTask.GetType().Name,
            RanAt = DateTime.Now,
        };

        database.CompletedMigrations.Add(completedMigration);
        await database.SaveChangesAsync();
    }

    private static List<T> getListOfInterfaceObjects<T>() where T : class
    {
        return Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.GetInterfaces().Contains(typeof(T)) && t.GetConstructor(Type.EmptyTypes) != null)
            .Select(t => Activator.CreateInstance(t) as T)
            .ToList()!;
    }
}