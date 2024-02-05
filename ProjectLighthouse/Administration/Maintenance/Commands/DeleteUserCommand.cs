#nullable enable
using System;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Maintenance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance.Commands;

public class DeleteUserCommand : ICommand
{
    public string Name() => "Delete User";
    public string[] Aliases()
        => new[]
        {
            "deleteUser", "wipeUser",
        };
    public string Arguments() => "<username/userId>";
    public int RequiredArgs() => 1;

    public async Task Run(IServiceProvider provider, string[] args, Logger logger)
    {
        DatabaseContext database = provider.GetRequiredService<DatabaseContext>();
        UserEntity? user = await database.Users.FirstOrDefaultAsync(u => u.Username.Length > 0 && u.Username == args[0]);
        if (user == null)
        {
            _ = int.TryParse(args[0], out int userId);
            user = await database.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                logger.LogError($"Could not find user by parameter '{args[0]}'", LogArea.Command);
                return;
            }
        }

        await database.RemoveUser(user);
        logger.LogSuccess($"Successfully deleted user {user.Username}", LogArea.Command);
    }
}