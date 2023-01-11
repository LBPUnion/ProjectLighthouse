#nullable enable
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.StorableLists.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers;

[ApiController]
[Authorize]
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
        GameToken token = this.GetToken();

        NPData? npData = await this.DeserializeBody<NPData>();
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
        GameToken token = this.GetToken();

        UserFriendData? friendStore = UserFriendStore.GetUserFriendData(token.UserId);

        if (friendStore == null)
            return this.Ok(LbpSerializer.BlankElement("myFriends"));

        string friends = "";
        foreach (int friendId in friendStore.FriendIds)
        {
            User? friend = await this.database.Users.Include(u => u.Location).FirstOrDefaultAsync(u => u.UserId == friendId);
            if (friend == null) continue;

            friends += friend.Serialize(token.GameVersion);
        }

        return this.Ok(LbpSerializer.StringElement("myFriends", friends));
    }
}