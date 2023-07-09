#nullable enable
using System;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Maintenance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance.Commands;

public class WipeTokensForUserCommand : ICommand
{
    public string Name() => "Wipe tokens for user";
    public string[] Aliases()
        => new[]
        {
            "wipeTokens", "wipeToken", "deleteTokens", "deleteToken", "removeTokens", "removeToken",
        };
    public string Arguments() => "<username/userId>";
    public int RequiredArgs() => 1;

    public async Task Run(IServiceProvider provider, string[] args, Logger logger)
    {
        DatabaseContext database = provider.GetRequiredService<DatabaseContext>();
        UserEntity? user = await database.Users.FirstOrDefaultAsync(u => u.Username == args[0]);
        if (user == null)
        {
            _ = int.TryParse(args[0], out int userId);
            user = await database.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                logger.LogError(@$"Could not find user by parameter '{args[0]}'", LogArea.Command);
                return;
            }
        }

        await database.GameTokens.RemoveWhere(t => t.UserId == user.UserId);
        await database.WebTokens.RemoveWhere(t => t.UserId == user.UserId);

        logger.LogSuccess(@$"Deleted all tokens for {user.Username} (id: {user.UserId}).", LogArea.Command);
    }
}