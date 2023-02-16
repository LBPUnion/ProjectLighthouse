using System;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;

namespace LBPUnion.ProjectLighthouse.Types.Maintenance;

public interface IRepeatingTask
{
    public string Name { get; }
    public TimeSpan RepeatInterval { get; }
    public DateTime LastRan { get; set; }

    public Task Run(DatabaseContext database);
}