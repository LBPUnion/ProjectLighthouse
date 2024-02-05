#nullable enable
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
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
[Produces("text/xml")]
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
        GameTokenEntity token = this.GetToken();

        NPData? npData = await this.DeserializeBody<NPData>();
        if (npData == null) return this.BadRequest();

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

        List<MinimalUserProfile> minimalFriends =
            friends.Select(u => new MinimalUserProfile
            {
                UserHandle = new NpHandle(u.Username, ""),
            }).ToList();

        return this.Ok(new FriendResponse(minimalFriends));
    }

    [HttpGet("myFriends")]
    public async Task<IActionResult> MyFriends()
    {
        GameTokenEntity token = this.GetToken();

        UserFriendData? friendStore = UserFriendStore.GetUserFriendData(token.UserId);

        GenericUserResponse<GameUser> response = new("myFriends", new List<GameUser>());

        if (friendStore == null)
            return this.Ok(response);

        foreach (int friendId in friendStore.FriendIds)
        {
            UserEntity? friend = await this.database.Users.FirstOrDefaultAsync(u => u.UserId == friendId);
            if (friend == null) continue;

            response.Users.Add(GameUser.CreateFromEntity(friend, token.GameVersion));
        }

        return this.Ok(response);
    }
}