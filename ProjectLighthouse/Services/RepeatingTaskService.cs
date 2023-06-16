#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Maintenance;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LBPUnion.ProjectLighthouse.Services;

public class RepeatingTaskService : BackgroundService
{
    private readonly IServiceProvider provider;
    private readonly List<IRepeatingTask> taskList;

    public RepeatingTaskService(IServiceProvider provider, List<IRepeatingTask> tasks)
    {
        this.provider = provider;
        this.taskList = tasks;

        Logger.Info("Initializing repeating tasks service", LogArea.Startup);
    }

    public bool TryGetNextTask(out IRepeatingTask? outTask)
    {
        TimeSpan smallestSpan = TimeSpan.MaxValue;
        IRepeatingTask? smallestTask = null;
        foreach (IRepeatingTask task in this.taskList)
        {
            TimeSpan smallestTimeRemaining = task.RepeatInterval.Subtract(DateTime.UtcNow.Subtract(task.LastRan));
            if (smallestTimeRemaining >= smallestSpan) continue;

            smallestSpan = smallestTimeRemaining;
            smallestTask = task;
        }
        outTask = smallestTask;

        return outTask != null;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        while (!stoppingToken.IsCancellationRequested)
        {
            if (!this.TryGetNextTask(out IRepeatingTask? task) || task == null)
            {
                // If we fail to fetch the next task then something has gone wrong and the service should halt
                Logger.Debug("Failed to fetch next smallest task", LogArea.Maintenance);
                return;
            }

            TimeSpan timeElapsedSinceRun = DateTime.UtcNow.Subtract(task.LastRan);

            // If the task's repeat interval hasn't elapsed
            if (timeElapsedSinceRun < task.RepeatInterval)
            {
                Logger.Debug($"Waiting {task.RepeatInterval.Subtract(timeElapsedSinceRun)} for {task.Name}", LogArea.Maintenance);
                await Task.Delay(task.RepeatInterval.Subtract(timeElapsedSinceRun), stoppingToken);
            }

            using IServiceScope scope = this.provider.CreateScope();
            DatabaseContext database = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            await task.Run(database);
            Logger.Debug($"Successfully ran task \"{task.Name}\"", LogArea.Maintenance);
            task.LastRan = DateTime.UtcNow;
        }
    }
}