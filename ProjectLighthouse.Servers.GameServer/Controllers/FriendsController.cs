#nullable enable
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.StorableLists.Stores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers;

[ApiController]
[Route("LITTLEBIGPLANETPS3_XML/")]
public class FriendsController : ControllerBase
{
    private readonly Database database;

    public FriendsController(Database database)
    {
        this.database = database;
    }

    [HttpPost("npdata")]
    public async Task<IActionResult> NPData()
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        this.Request.Body.Position = 0;
        string bodyString = await new StreamReader(this.Request.Body).ReadToEndAsync();

        XmlSerializer serializer = new(typeof(NPData));
        NPData? npData = (NPData?)serializer.Deserialize(new StringReader(bodyString));
        if (npData == null) return this.BadRequest();

        SanitizationHelper.SanitizeStringsInClass(npData);

        List<User> friends = new();
        foreach (string friendName in npData.Friends)
        {
            User? friend = await this.database.Users.FirstOrDefaultAsync(u => u.Username == friendName);
            if (friend == null) continue;

            friends.Add(friend);
        }

        List<int> blockedUsers = new();
        foreach (string blockedUserName in npData.BlockedUsers)
        {
            User? blockedUser = await this.database.Users.FirstOrDefaultAsync(u => u.Username == blockedUserName);
            if (blockedUser == null) continue;

            blockedUsers.Add(blockedUser.UserId);
        }

        UserFriendData friendStore = UserFriendStore.GetUserFriendData(token.UserId) ?? UserFriendStore.CreateUserFriendData(token.UserId);

        friendStore.FriendIds = friends.Select(u => u.UserId).ToList();
        friendStore.BlockedIds = blockedUsers;

        UserFriendStore.UpdateFriendData(friendStore);

        string friendsSerialized = friends.Aggregate(string.Empty, (current, user1) => current + LbpSerializer.StringElement("npHandle", user1.Username));

        return this.Ok(LbpSerializer.StringElement("npdata", LbpSerializer.StringElement("friends", friendsSerialized)));
    }

    [HttpGet("myFriends")]
    public async Task<IActionResult> MyFriends()
    {
        (User, GameToken)? userAndToken = await this.database.UserAndGameTokenFromRequest(this.Request);

        if (userAndToken == null) return this.StatusCode(403, "");

        // ReSharper disable once PossibleInvalidOperationException
        User user = userAndToken.Value.Item1;
        GameToken gameToken = userAndToken.Value.Item2;

        UserFriendData? friendStore = UserFriendStore.GetUserFriendData(user.UserId);

        if (friendStore == null)
            return this.Ok(LbpSerializer.BlankElement("myFriends"));

        string friends = "";
        foreach (int friendId in friendStore.FriendIds)
        {
            User? friend = await this.database.Users.Include(u => u.Location).FirstOrDefaultAsync(u => u.UserId == friendId);
            if (friend == null) continue;

            friends += friend.Serialize(gameToken.GameVersion);
        }

        return this.Ok(LbpSerializer.StringElement("myFriends", friends));
    }
}