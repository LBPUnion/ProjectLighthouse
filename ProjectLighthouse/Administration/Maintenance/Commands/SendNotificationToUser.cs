using System;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Maintenance;
using Microsoft.Extensions.DependencyInjection;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance.Commands;

public class SendNotificationToUser : ICommand
{
    public string Name() => "Send Notification to User";

    public string[] Aliases() => new[]
    {
        "sendUserNotification", "sendNotification", "sendUserNotif", "sendNotif",
    };

    public string Arguments() => "<user id>:<notification text>";

    public int RequiredArgs() => 1;

    public async Task Run(IServiceProvider provider, string[] args, Logger logger)
    {
        DatabaseContext database = provider.GetRequiredService<DatabaseContext>();

        int userId;
        string text;

        try
        {
            userId = int.Parse(args[0].Split(':')[0]);
            text = args[0].Split(':')[1];
        }
        catch (Exception)
        {
            logger.LogError("Failed to parse arguments.", LogArea.Maintenance);
            return;
        }

        if (database.Users.FirstOrDefault(u => u.UserId == userId) == null)
        {
            logger.LogError($"User with ID {userId} does not exist.", LogArea.Maintenance);
            return;
        }

        logger.LogInfo($"Sending notification to user ID {userId}...", LogArea.Maintenance);
        await database.SendNotification(userId, text);
    }
}