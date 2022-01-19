#nullable enable
using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Maintenance.Commands;

[UsedImplicitly]
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

    public async Task Run(string[] args)
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
                Console.WriteLine($"Could not find user by parameter '{args[0]}'");
                return;
            }

        user.IsAdmin = true;
        await this.database.SaveChangesAsync();

        Console.WriteLine($"The user {user.Username} (id: {user.UserId}) is now an admin.");
    }
}