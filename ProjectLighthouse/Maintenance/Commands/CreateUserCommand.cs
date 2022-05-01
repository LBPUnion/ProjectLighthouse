#nullable enable
using System.Threading.Tasks;
using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Maintenance.Commands;

[UsedImplicitly]
public class CreateUserCommand : ICommand
{
    private readonly Database _database = new();

    public async Task Run(string[] args)
    {
        string onlineId = args[0];
        string password = args[1];

        password = CryptoHelper.Sha256Hash(password);

        User? user = await this._database.Users.FirstOrDefaultAsync(u => u.Username == onlineId);
        if (user == null)
        {
            user = await this._database.CreateUser(onlineId, CryptoHelper.BCryptHash(password));
            Logger.LogSuccess($"Created user {user.UserId} with online ID (username) {user.Username} and the specified password.", LogArea.Login);

            user.PasswordResetRequired = true;
            Logger.LogInfo("This user will need to reset their password when they log in.", LogArea.Login);

            await this._database.SaveChangesAsync();
            Logger.LogInfo("Database changes saved.", LogArea.Database);
        }
        else
        {
            Logger.LogError("A user with this username already exists.", LogArea.Login);
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