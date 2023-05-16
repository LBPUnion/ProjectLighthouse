#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Maintenance;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance.Commands;

public class WipeTokensForUserCommand : ICommand
{
    private readonly DatabaseContext database = DatabaseContext.CreateNewInstance();

    public string Name() => "Wipe tokens for user";
    public string[] Aliases()
        => new[]
        {
            "wipeTokens", "wipeToken", "deleteTokens", "deleteToken", "removeTokens", "removeToken",
        };
    public string Arguments() => "<username/userId>";
    public int RequiredArgs() => 1;
    public async Task Run(string[] args, Logger logger)
    {
        UserEntity? user = await this.database.Users.FirstOrDefaultAsync(u => u.Username == args[0]);
        if (user == null)
            try
            {
                user = await this.database.Users.FirstOrDefaultAsync(u => u.UserId == Convert.ToInt32(args[0]));
                if (user == null) throw new Exception();
            }
            catch
            {
                Console.WriteLine(@$"Could not find user by parameter '{args[0]}'");
                return;
            }

        this.database.GameTokens.RemoveRange(this.database.GameTokens.Where(t => t.UserId == user.UserId));
        this.database.WebTokens.RemoveRange(this.database.WebTokens.Where(t => t.UserId == user.UserId));

        await this.database.SaveChangesAsync();

        Console.WriteLine(@$"Deleted all tokens for {user.Username} (id: {user.UserId}).");
    }
}