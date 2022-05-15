using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Match.Rooms;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Controllers.Debug;

[ApiController]
[Route("debug/roomVisualizer")]
public class RoomVisualizerController : ControllerBase
{
    private readonly Database database;

    public RoomVisualizerController(Database database)
    {
        this.database = database;
    }

    [HttpGet("createFakeRoom")]
    public async Task<IActionResult> CreateFakeRoom()
    {
        #if !DEBUG
        return this.NotFound();
        #else
        List<int> users = await this.database.Users.OrderByDescending(_ => EF.Functions.Random()).Take(2).Select(u => u.UserId).ToListAsync();
        RoomHelper.CreateRoom(users, GameVersion.LittleBigPlanet2, Platform.PS3);

        foreach (int user in users)
        {
            MatchHelper.SetUserLocation(user, "127.0.0.1");
        }
        return this.Redirect("/debug/roomVisualizer");
        #endif
    }

    [HttpGet("deleteRooms")]
    public IActionResult DeleteRooms()
    {
        #if !DEBUG
        return this.NotFound();
        #else
        RoomHelper.Rooms.RemoveAll();
        return this.Redirect("/debug/roomVisualizer");
        #endif
    }
}