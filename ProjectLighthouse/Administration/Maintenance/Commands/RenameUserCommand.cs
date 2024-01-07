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

public class RenameUserCommand : ICommand
{
    public string Name() => "Rename User";
    public string[] Aliases()
        => new[]
        {
            "renameUser",
        };
    public string Arguments() => "<username/userId> <newUsername>";
    public int RequiredArgs() => 2;

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
                logger.LogError($"Could not find user by parameter '{args[0]}'", LogArea.Command);
                return;
            }
        }

        // prevent the placeholder user from being renamed
        if (user.Username.Length == 0)
        {
            logger.LogError("Cannot rename the placeholder user", LogArea.Command);
            return;
        }

        user.Username = args[1];
        await database.SaveChangesAsync();

        logger.LogSuccess($"The username for user {user.Username} (id: {user.UserId}) has been changed.",
            LogArea.Command);
    }
}