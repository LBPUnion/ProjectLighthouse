#nullable enable
using System;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance.Commands;

public class MakeUserAdminCommand : ICommand
{
    private readonly Database database = new();

    public string Name() => "Make User Admin";
    public string[] Aliases()
        => new[]
        {
            "makeAdmin",
        };
    public string Arguments() => "<username/userId>";
    public int RequiredArgs() => 1;

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

        user.IsAdmin = true;
        await this.database.SaveChangesAsync();

        logger.LogSuccess($"The user {user.Username} (id: {user.UserId}) is now an admin.", LogArea.Command);
    }
}