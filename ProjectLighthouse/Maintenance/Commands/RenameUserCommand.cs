#nullable enable
using System;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Maintenance.Commands;

public class RenameUserCommand : ICommand
{
    private readonly Database database = new();
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

        user.Username = args[1];
        await this.database.SaveChangesAsync();

        Console.WriteLine($"The username for user {user.Username} (id: {user.UserId}) has been changed.");
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