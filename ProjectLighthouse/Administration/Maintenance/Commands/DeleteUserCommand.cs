#nullable enable
using System;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Maintenance;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance.Commands;

public class DeleteUserCommand : ICommand
{
    private readonly DatabaseContext database = DatabaseContext.CreateNewInstance();
    public string Name() => "Delete User";
    public string[] Aliases()
        => new[]
        {
            "deleteUser", "wipeUser",
        };
    public string Arguments() => "<username/userId>";
    public int RequiredArgs() => 1;
    public async Task Run(string[] args, Logger logger)
    {
        UserEntity? user = await this.database.Users.FirstOrDefaultAsync(u => u.Username.Length > 0 && u.Username == args[0]);
        if (user == null)
            try
            {
                user = await this.database.Users.FirstOrDefaultAsync(u => u.UserId == Convert.ToInt32(args[0]));
                if (user == null) throw new Exception();
            }
            catch
            {
                logger.LogError($"Could not find user by parameter '{args[0]}'", LogArea.Command);
                return;
            }

        await this.database.RemoveUser(user);
        logger.LogSuccess($"Successfully deleted user {user.Username}", LogArea.Command);
    }
}