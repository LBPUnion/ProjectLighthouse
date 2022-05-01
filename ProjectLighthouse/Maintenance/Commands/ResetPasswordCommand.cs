#nullable enable
using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Maintenance.Commands;

[UsedImplicitly]
public class ResetPasswordCommand : ICommand
{
    private readonly Database database = new();
    public string Name() => "Reset Password";
    public string[] Aliases()
        => new[]
        {
            "setPassword", "resetPassword", "passwd", "password",
        };
    public string Arguments() => "<username/userId> <sha256/plaintext>";
    public int RequiredArgs() => 2;

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
        string password = args[1];
        if (password.Length != 64) password = CryptoHelper.Sha256Hash(password);

        user.Password = CryptoHelper.BCryptHash(password);
        user.PasswordResetRequired = true;

        await this.database.SaveChangesAsync();

        Console.WriteLine($"The password for user {user.Username} (id: {user.UserId}) has been reset.");
    }
}