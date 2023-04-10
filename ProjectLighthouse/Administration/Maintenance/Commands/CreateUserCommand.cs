#nullable enable
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Maintenance;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance.Commands;

public class CreateUserCommand : ICommand
{
    private readonly DatabaseContext _database = DatabaseContext.CreateNewInstance();

    public async Task Run(string[] args, Logger logger)
    {
        string onlineId = args[0];
        string password = args[1];

        password = CryptoHelper.Sha256Hash(password);

        UserEntity? user = await this._database.Users.FirstOrDefaultAsync(u => u.Username == onlineId);
        if (user == null)
        {
            user = await this._database.CreateUser(onlineId, CryptoHelper.BCryptHash(password));
            logger.LogSuccess($"Created user {user.UserId} with online ID (username) {user.Username} and the specified password.", LogArea.Command);

            user.PasswordResetRequired = true;
            logger.LogInfo("This user will need to reset their password when they log in.", LogArea.Command);

            await this._database.SaveChangesAsync();
            logger.LogInfo("Database changes saved.", LogArea.Command);
        }
        else
        {
            logger.LogError("A user with this username already exists.", LogArea.Command);
        }
    }

    public string Name() => "Create New User";

    public string[] Aliases()
        => new[]
        {
            "useradd", "adduser", "newuser", "createUser",
        };

    public string Arguments() => "<OnlineID> <Password>";

    public int RequiredArgs() => 2;
}