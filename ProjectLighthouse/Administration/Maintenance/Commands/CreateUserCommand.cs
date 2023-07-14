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

public class CreateUserCommand : ICommand
{
    public string Name() => "Create New User";
    public string[] Aliases() =>
        new[]
        {
            "useradd", "adduser", "newuser", "createUser",
        };
    public string Arguments() => "<OnlineID> <Password>";
    public int RequiredArgs() => 2;

    public async Task Run(IServiceProvider provider, string[] args, Logger logger)
    {

        DatabaseContext database = provider.GetRequiredService<DatabaseContext>();
        string onlineId = args[0];
        string password = args[1];

        password = CryptoHelper.Sha256Hash(password);

        UserEntity? user = await database.Users.FirstOrDefaultAsync(u => u.Username == onlineId);
        if (user != null)
        {
            logger.LogError("A user with this username already exists.", LogArea.Command);
            return;
        }

        user = await database.CreateUser(onlineId, CryptoHelper.BCryptHash(password));
        logger.LogSuccess(
            $"Created user {user.UserId} with online ID (username) {user.Username} and the specified password.",
            LogArea.Command);

        user.PasswordResetRequired = true;
        logger.LogInfo("This user will need to reset their password when they log in.", LogArea.Command);

        await database.SaveChangesAsync();
        logger.LogInfo("Database changes saved.", LogArea.Command);
    }
}