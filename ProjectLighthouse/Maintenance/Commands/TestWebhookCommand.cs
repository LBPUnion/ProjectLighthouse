using System.Threading.Tasks;
using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Helpers;

namespace LBPUnion.ProjectLighthouse.Maintenance.Commands;

[UsedImplicitly]
public class TestWebhookCommand : ICommand
{
    public async Task Run(string[] args)
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