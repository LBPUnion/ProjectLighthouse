using System;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Types.Maintenance;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance.RepeatingTasks;

public class CleanupRoomsTask : IRepeatingTask
{
    public string Name => "Cleanup Rooms";
    public TimeSpan RepeatInterval => TimeSpan.FromSeconds(10);
    public DateTime LastRan { get; set; }
    public Task Run(DatabaseContext database) => RoomHelper.CleanupRooms();
}