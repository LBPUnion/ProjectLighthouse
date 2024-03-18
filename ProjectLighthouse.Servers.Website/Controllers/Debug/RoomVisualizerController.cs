using LBPUnion.ProjectLighthouse.Database;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Controllers.Debug;

[ApiController]
[Route("debug/roomVisualizer")]
public class RoomVisualizerController : ControllerBase
{
    private readonly DatabaseContext database;

    public RoomVisualizerController(DatabaseContext database)
    {
        this.database = database;
    }

    [HttpGet("createFakeRoom")]
    public async Task<IActionResult> CreateFakeRoom()
    {
        #if !DEBUG
        await Task.FromResult(0);
        return this.NotFound();
        #else
        List<int> users = await this.database.Users.OrderByDescending(_ => EF.Functions.Random()).Take(2).Select(u => u.UserId).ToListAsync();
        RoomHelper.CreateRoom(users, GameVersion.LittleBigPlanet2, Platform.PS3);

        return this.Redirect("/debug/roomVisualizer");
        #endif
    }

    [HttpGet("deleteRooms")]
    public IActionResult DeleteRooms()
    {
        #if !DEBUG
        return this.NotFound();
        #else
        lock(RoomHelper.RoomLock) RoomHelper.Rooms.RemoveAll();
        return this.Redirect("/debug/roomVisualizer");
        #endif
    }

    [HttpGet("createRoomsWithDuplicatePlayers")]
    public async Task<IActionResult> CreateRoomsWithDuplicatePlayers()
    {
        #if !DEBUG
        await Task.FromResult(0);
        return this.NotFound();
        #else
        List<int> users = await this.database.Users.OrderByDescending(_ => EF.Functions.Random()).Take(1).Select(u => u.UserId).ToListAsync();
        RoomHelper.CreateRoom(users, GameVersion.LittleBigPlanet2, Platform.PS3);
        RoomHelper.CreateRoom(users, GameVersion.LittleBigPlanet2, Platform.PS3);
        return this.Redirect("/debug/roomVisualizer");
        #endif
    }
}