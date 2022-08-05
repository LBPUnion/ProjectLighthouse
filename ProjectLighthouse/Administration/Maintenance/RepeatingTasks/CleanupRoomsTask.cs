using System;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Match.Rooms;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance.RepeatingTasks;

public class CleanupRoomsTask : IRepeatingTask
{
    public string Name => "Cleanup Rooms";
    public TimeSpan RepeatInterval => TimeSpan.FromSeconds(10);
    public DateTime LastRan { get; set; }
    public async Task Run(Database database) => RoomHelper.CleanupRooms();
}