using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Maintenance;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance.Commands;

public class TestWebhookCommand : ICommand
{
    public async Task Run(string[] args, Logger logger)
    {
        await WebhookHelper.SendWebhook("Testing 123", "Someone is testing the Discord webhook from the admin panel.");
    }
    public string Name() => "Test Discord Webhook";
    public string[] Aliases()
        => new[]
        {
            "testWebhook", "testDiscordWebhook",
        };
    public string Arguments() => "";
    public int RequiredArgs() => 0;
}