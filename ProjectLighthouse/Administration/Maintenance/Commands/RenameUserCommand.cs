#nullable enable
using System;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance.Commands;

public class RenameUserCommand : ICommand
{
    private readonly Database database = new();
    public async Task Run(string[] args, Logger logger)
    {
        User? user = await this.database.Users.FirstOrDefaultAsync(u => u.Username == args[0]);
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

        user.Username = args[1];
        await this.database.SaveChangesAsync();

        logger.LogSuccess($"The username for user {user.Username} (id: {user.UserId}) has been changed.", LogArea.Command);
    }
    public string Name() => "Rename User";
    public string[] Aliases()
        => new[]
        {
            "renameUser",
        };
    public string Arguments() => "<username/userId> <newUsername>";
    public int RequiredArgs() => 2;
}