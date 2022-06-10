#nullable enable
using System;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance;

#region Base Type
public abstract class SetUserPermissionLevelCommand : ICommand
{
    private readonly PermissionLevel permissionLevel;
    
    protected SetUserPermissionLevelCommand(PermissionLevel permissionLevel)
    {
        this.permissionLevel = permissionLevel;
    }

    private readonly Database database = new();
    public abstract string Name();
    public abstract string[] Aliases();
    
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
                logger.LogError($"Could not find user by parameter '{args[0]}'",
                    LogArea.Command);
                return;
            }

        user.PermissionLevel = this.permissionLevel;
        await this.database.SaveChangesAsync();

        logger.LogSuccess($"The user {user.Username} (id: {user.UserId}) is now {this.permissionLevel}.",
            LogArea.Command);
    }
}
#endregion

#region Implementations

public class SetUserAdminCommand : SetUserPermissionLevelCommand
{
    public SetUserAdminCommand() : base(PermissionLevel.Administrator)
    {}
    public override string Name() => "Make User Admin";
    public override string[] Aliases() => new []{"make-admin",};
}

public class SetUserModeratorCommand : SetUserPermissionLevelCommand
{
    public SetUserModeratorCommand() : base(PermissionLevel.Moderator)
    {}
    public override string Name() => "Make User Moderator";
    public override string[] Aliases() => new[] { "make-moderator", };
}

public class BanUserCommand : SetUserPermissionLevelCommand
{
    public BanUserCommand() : base(PermissionLevel.Banned)
    {}
    public override string Name() => "Ban User";
    public override string[] Aliases() => new[] { "ban", };
}

public class DemoteUserCommand : SetUserPermissionLevelCommand
{
    public DemoteUserCommand() : base(PermissionLevel.Default)
    {}
    public override string Name() => "Demote User";
    public override string[] Aliases() => new[] { "demote", };
}
#endregion