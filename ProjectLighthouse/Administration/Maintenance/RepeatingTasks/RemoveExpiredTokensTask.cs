using System;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Maintenance;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance.RepeatingTasks;

public class RemoveExpiredTokensTask : IRepeatingTask
{
    public string Name => "Remove Expired Tokens";
    public TimeSpan RepeatInterval => TimeSpan.FromHours(1);
    public DateTime LastRan { get; set; }

    public Task Run(Database database) => database.RemoveExpiredTokens();
}