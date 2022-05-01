#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Helpers.Extensions;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Match;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Controllers.GameApi.Matching;

[ApiController]
[Route("LITTLEBIGPLANETPS3_XML/")]
[Produces("text/xml")]
public class MatchController : ControllerBase
{
    private readonly Database database;

    public MatchController(Database database)
    {
        this.database = database;
    }

    [HttpPost("match")]
    [Produces("text/plain")]
    public async Task<IActionResult> Match()
    {
        (User, GameToken)? userAndToken = await this.database.UserAndGameTokenFromRequest(this.Request);

        if (userAndToken == null) return this.StatusCode(403, "");

        // ReSharper disable once PossibleInvalidOperationException
        User user = userAndToken.Value.Item1;
        GameToken gameToken = userAndToken.Value.Item2;

        #region Parse match data

        // Example POST /match: [UpdateMyPlayerData,["Player":"FireGamer9872"]]

        string bodyString = await new StreamReader(this.Request.Body).ReadToEndAsync();

        if (bodyString.Length == 0 || bodyString[0] != '[') return this.BadRequest();

        Logger.LogInfo("Received match data: " + bodyString, LogArea.Match);

        IMatchData? matchData;
        try
        {
            matchData = MatchHelper.Deserialize(bodyString);
        }
        catch(Exception e)
        {
            Logger.LogError("Exception while parsing matchData: ", LogArea.Match);
            Logger.LogError(e.ToDetailedException(), LogArea.Match);

            return this.BadRequest();
        }

        if (matchData == null)
        {
            Logger.LogError($"Could not parse match data: {nameof(matchData)} is null", LogArea.Match);
            return this.BadRequest();
        }

        Logger.LogInfo($"Parsed match from {user.Username} (type: {matchData.GetType()})", LogArea.Match);

        #endregion

        await LastContactHelper.SetLastContact(user, gameToken.GameVersion, gameToken.Platform);

        #region Process match data

        if (matchData is UpdateMyPlayerData playerData)
        {
            MatchHelper.SetUserLocation(user.UserId, gameToken.UserLocation);
            Room? room = RoomHelper.FindRoomByUser(user, gameToken.GameVersion, gameToken.Platform, true);

            if (playerData.RoomState != null)
                if (room != null && Equals(room.Host, user))
                    room.State = (RoomState)playerData.RoomState;
        }

        // Check how many people are online in release builds, disabled for debug for ..well debugging.
        #if DEBUG
        if (matchData is FindBestRoom diveInData)
            #else
        if (matchData is FindBestRoom diveInData && MatchHelper.UserLocations.Count > 1)
            #endif
        {
            FindBestRoomResponse? response = RoomHelper.FindBestRoom
                (user, gameToken.GameVersion, diveInData.RoomSlot, gameToken.Platform, gameToken.UserLocation);

            if (response == null) return this.NotFound();

            string serialized = JsonSerializer.Serialize(response, typeof(FindBestRoomResponse));
            foreach (Player player in response.Players) MatchHelper.AddUserRecentlyDivedIn(user.UserId, player.User.UserId);

            return this.Ok($"[{{\"StatusCode\":200}},{serialized}]");
        }

        if (matchData is CreateRoom createRoom && MatchHelper.UserLocations.Count >= 1)
        {
            List<User> users = new();
            foreach (string playerUsername in createRoom.Players)
            {
                User? player = await this.database.Users.FirstOrDefaultAsync(u => u.Username == playerUsername);
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (player != null) users.Add(player);
                else return this.BadRequest();
            }

            // Create a new one as requested
            RoomHelper.CreateRoom(users, gameToken.GameVersion, gameToken.Platform, createRoom.RoomSlot);
        }

        if (matchData is UpdatePlayersInRoom updatePlayersInRoom)
        {
            Room? room = RoomHelper.Rooms.FirstOrDefault(r => r.Host == user);

            if (room != null)
            {
                List<User> users = new();
                foreach (string playerUsername in updatePlayersInRoom.Players)
                {
                    User? player = await this.database.Users.FirstOrDefaultAsync(u => u.Username == playerUsername);
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (player != null) users.Add(player);
                    else return this.BadRequest();
                }

                room.Players = users;
                RoomHelper.CleanupRooms(null, room);
            }
        }

        #endregion

        return this.Ok("[{\"StatusCode\":200}]");
    }
}