#nullable enable
using System;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Maintenance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance.Commands;

public class ResetPasswordCommand : ICommand
{
    public string Name() => "Reset Password";
    public string[] Aliases()
        => new[]
        {
            "setPassword", "resetPassword", "passwd", "password",
        };
    public string Arguments() => "<username/userId> <sha256/plaintext>";
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

        string password = args[1];
        if (password.Length != 64) password = CryptoHelper.Sha256Hash(password);

        user.Password = CryptoHelper.BCryptHash(password);
        user.PasswordResetRequired = true;

        await database.SaveChangesAsync();

        logger.LogSuccess($"The password for user {user.Username} (id: {user.UserId}) has been reset.", LogArea.Command);
    }
}