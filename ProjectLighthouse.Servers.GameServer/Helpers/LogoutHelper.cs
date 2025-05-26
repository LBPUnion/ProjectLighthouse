using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Helpers;

public class LogoutHelper
{
    public static async Task Logout(GameTokenEntity token, UserEntity user, DatabaseContext database)
    {
        user.LastLogout = TimeHelper.TimestampMillis;

        await database.GameTokens.RemoveWhere(t => t.TokenId == token.TokenId);
        await database.LastContacts.RemoveWhere(c => c.UserId == token.UserId);
    }
}