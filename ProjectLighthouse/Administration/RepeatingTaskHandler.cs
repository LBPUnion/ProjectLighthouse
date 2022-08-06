#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Administration.Maintenance;
using LBPUnion.ProjectLighthouse.Logging;

namespace LBPUnion.ProjectLighthouse.Administration;

public static class RepeatingTaskHandler
{
    private static bool initialized = false;

    public static void Initialize()
    {
        if (initialized) throw new InvalidOperationException("RepeatingTaskHandler was initialized twice");

        initialized = true;
        Task.Factory.StartNew(taskLoop);
    }

    [SuppressMessage("ReSharper", "FunctionNeverReturns")]
    private static async Task taskLoop()
    {
        Queue<IRepeatingTask> taskQueue = new();
        foreach (IRepeatingTask task in MaintenanceHelper.RepeatingTasks) taskQueue.Enqueue(task);

        Database database = new();

        while (true)
        {
            try
            {
                if (!taskQueue.TryDequeue(out IRepeatingTask? task))
                {
                    Thread.Sleep(100);
                    continue;
                }

                Debug.Assert(task != null);

                if ((task.LastRan + task.RepeatInterval) <= DateTime.Now)
                {
                    await task.Run(database);
                    task.LastRan = DateTime.Now;

                    Logger.Debug($"Ran task \"{task.Name}\"", LogArea.Maintenace);
                }

                taskQueue.Enqueue(task);
                Thread.Sleep(500); // Doesn't need to be that precise.
            }
            catch
            {
                // ignored
            }
        }
    }
}