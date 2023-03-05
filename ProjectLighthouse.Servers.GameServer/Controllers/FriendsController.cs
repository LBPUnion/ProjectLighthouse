#nullable enable
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Users;
using LBPUnion.ProjectLighthouse.StorableLists.Stores;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Serialization;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers;

[ApiController]
[Authorize]
[Route("LITTLEBIGPLANETPS3_XML/")]
public class FriendsController : ControllerBase
{
    private readonly DatabaseContext database;

    public FriendsController(DatabaseContext database)
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

        List<UserEntity> friends = new();
        foreach (string friendName in npData.Friends ?? new List<string>())
        {
            UserEntity? friend = await this.database.Users.FirstOrDefaultAsync(u => u.Username == friendName);
            if (friend == null) continue;

            friends.Add(friend);
        }

        List<int> blockedUsers = new();
        foreach (string blockedUserName in npData.BlockedUsers ?? new List<string>())
        {
            UserEntity? blockedUser = await this.database.Users.FirstOrDefaultAsync(u => u.Username == blockedUserName);
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

        UserListResponse response = new("myFriends", new List<UserProfile>());

        if (friendStore == null)
            return this.Ok(response);

        foreach (int friendId in friendStore.FriendIds)
        {
            UserEntity? friend = await this.database.Users.FirstOrDefaultAsync(u => u.UserId == friendId);
            if (friend == null) continue;

            response.Users.Add(UserProfile.CreateFromEntity(friend));
        }

        return this.Ok(response);
    }
}