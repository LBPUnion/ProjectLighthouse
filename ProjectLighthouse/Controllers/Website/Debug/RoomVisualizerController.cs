using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Controllers.Website.Debug;

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
        List<User> users = await this.database.Users.OrderByDescending(_ => EF.Functions.Random()).Take(2).ToListAsync();
        RoomHelper.CreateRoom(users, GameVersion.LittleBigPlanet2);

        foreach (User user in users)
        {
            MatchHelper.SetUserLocation(user.UserId, "127.0.0.1");
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
        RoomHelper.Rooms.RemoveAll(_ => true);
        return this.Redirect("/debug/roomVisualizer");
        #endif
    }
}