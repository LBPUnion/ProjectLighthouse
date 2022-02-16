#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Profiles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Controllers.GameApi;

[ApiController]
[Route("LITTLEBIGPLANETPS3_XML/")]
[Produces("text/xml")]
public class UserController : ControllerBase
{
    private readonly Database database;

    public UserController(Database database)
    {
        this.database = database;
    }

    private async Task<string?> getSerializedUser(string username, GameVersion gameVersion = GameVersion.LittleBigPlanet1)
    {
        User? user = await this.database.Users.Include(u => u.Location).FirstOrDefaultAsync(u => u.Username == username);
        return user?.Serialize(gameVersion);
    }

    [HttpGet("user/{username}")]
    public async Task<IActionResult> GetUser(string username)
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        string? user = await this.getSerializedUser(username, token.GameVersion);
        if (user == null) return this.NotFound();

        return this.Ok(user);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUserAlt([FromQuery] string[] u)
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        List<string?> serializedUsers = new();
        foreach (string userId in u) serializedUsers.Add(await this.getSerializedUser(userId, token.GameVersion));

        string serialized = serializedUsers.Aggregate(string.Empty, (current, user) => user == null ? current : current + user);

        return this.Ok(LbpSerializer.StringElement("users", serialized));
    }

    [HttpPost("updateUser")]
    public async Task<IActionResult> UpdateUser()
    {
        (User, GameToken)? userAndToken = await this.database.UserAndGameTokenFromRequest(this.Request);

        if (userAndToken == null) return this.StatusCode(403, "");

        // ReSharper disable once PossibleInvalidOperationException
        User user = userAndToken.Value.Item1;
        GameToken gameToken = userAndToken.Value.Item2;

        XmlReaderSettings settings = new()
        {
            Async = true, // this is apparently not default
        };

        bool locationChanged = false;

        // this is an absolute mess, but necessary because LBP only sends what changed
        //
        // example for changing profile card location:
        // <updateUser>
        //     <location>
        //         <x>1234</x>
        //         <y>1234</y>
        //     </location>
        // </updateUser>
        //
        // example for changing biography:
        // <updateUser>
        //     <biography>biography stuff</biography>
        // </updateUser>
        //
        // if you find a way to make it not stupid feel free to replace this
        using (XmlReader reader = XmlReader.Create(this.Request.Body, settings))
        {
            List<string> path = new(); // you can think of this as a file path in the XML, like <updateUser> -> <location> -> <x>
            while (await reader.ReadAsync()) // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        path.Add(reader.Name);
                        break;
                    case XmlNodeType.Text:
                        switch (path[1])
                        {
                            case "biography":
                            {
                                user.Biography = await reader.GetValueAsync();
                                break;
                            }
                            case "location":
                            {
                                locationChanged = true; // if we're here then we're probably about to change the location.
                                // ReSharper disable once ConvertIfStatementToSwitchStatement
                                if (path[2] == "x")
                                    user.Location.X = Convert.ToInt32
                                        (await reader.GetValueAsync()); // GetValue only returns a string, i guess we just hope its a number lol
                                else if (path[2] == "y") user.Location.Y = Convert.ToInt32(await reader.GetValueAsync());
                                break;
                            }
                            case "icon":
                            {
                                user.IconHash = await reader.GetValueAsync();
                                break;
                            }
                            case "planets":
                            {
                                switch (gameToken.GameVersion)
                                {
                                    case GameVersion.LittleBigPlanet2: // LBP2 planets will apply to LBP3
                                    {
                                        user.PlanetHashLBP2 = await reader.GetValueAsync();
                                        user.PlanetHashLBP3 = await reader.GetValueAsync();
                                        break;
                                    }
                                    case GameVersion.LittleBigPlanet3: // LBP3 and vita can only apply to their own games, only set 1 here
                                    {
                                        user.PlanetHashLBP3 = await reader.GetValueAsync();
                                        break;
                                    }
                                    case GameVersion.LittleBigPlanetVita:
                                    {
                                        user.PlanetHashLBPVita = await reader.GetValueAsync();
                                        break;
                                    }
                                    case GameVersion.LittleBigPlanet1:
                                    case GameVersion.LittleBigPlanetPSP:
                                    case GameVersion.Unknown:
                                    default: // The rest do not support custom earths.
                                    {
                                        throw new ArgumentException($"invalid gameVersion {gameToken.GameVersion} for setting earth");
                                    }
                                }
                                break;
                            }
                            case "yay2":
                            {
                                user.YayHash = await reader.GetValueAsync();
                                break;
                            }
                            case "meh2":
                            {
                                user.MehHash = await reader.GetValueAsync();
                                break;
                            }
                            case "boo2":
                            {
                                user.BooHash = await reader.GetValueAsync();
                                break;
                            }
                        }

                        break;
                    case XmlNodeType.EndElement:
                        path.RemoveAt(path.Count - 1);
                        break;
                }
        }

        // the way location on a user card works is stupid and will not save with the way below as-is, so we do the following:
        if (locationChanged) // only modify the database if we modify here
        {
            Location? l = await this.database.Locations.FirstOrDefaultAsync(l => l.Id == user.LocationId); // find the location in the database again

            if (l == null) throw new Exception("this shouldn't happen ever but we handle this");

            // set the location in the database to the one we modified above
            l.X = user.Location.X;
            l.Y = user.Location.Y;

            // now both are in sync, and will update in the database.
        }

        if (this.database.ChangeTracker.HasChanges()) await this.database.SaveChangesAsync(); // save the user to the database if we changed anything
        return this.Ok();
    }

    [HttpPost("update_my_pins")]
    public async Task<IActionResult> UpdateMyPins()
    {
        User? user = await this.database.UserFromGameRequest(this.Request);
        if (user == null) return this.StatusCode(403, "");

        string pinsString = await new StreamReader(this.Request.Body).ReadToEndAsync();
        Pins? pinJson = JsonSerializer.Deserialize<Pins>(pinsString);
        if (pinJson == null) return this.BadRequest();

        // Sometimes the update gets called periodically as pin progress updates via playing,
        // may not affect equipped profile pins however, so check before setting it.
        string currentPins = user.Pins;
        string newPins = string.Join(",", pinJson.ProfilePins);

        if (string.Equals(currentPins, newPins)) return this.Ok("[{\"StatusCode\":200}]");

        user.Pins = newPins;
        await this.database.SaveChangesAsync();

        return this.Ok("[{\"StatusCode\":200}]");
    }
}