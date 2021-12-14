#nullable enable
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kettu;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Maintenance.Commands
{
    [UsedImplicitly]
    public class CreateUserCommand : ICommand
    {
        private readonly Database _database = new();

        public async Task Run(string[] args)
        {
            string onlineId = args[0];
            string password = args[1];

            password = HashHelper.Sha256Hash(password);

            User? user = await this._database.Users.FirstOrDefaultAsync(u => u.Username == onlineId);
            if (user == null)
            {
                user = await this._database.CreateUser(onlineId, HashHelper.BCryptHash(password));
                Logger.Log($"Created user {user.UserId} with online ID (username) {user.Username} and the specified password.", LoggerLevelLogin.Instance);

                user.PasswordResetRequired = true;
                Logger.Log("This user will need to reset their password when they log in.", LoggerLevelLogin.Instance);

                await this._database.SaveChangesAsync();
                Logger.Log("Database changes saved.", LoggerLevelDatabase.Instance);
            }
            else
            {
                Logger.Log("A user with this username already exists.", LoggerLevelLogin.Instance);
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
}