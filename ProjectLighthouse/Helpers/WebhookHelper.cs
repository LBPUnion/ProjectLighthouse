using System.Threading.Tasks;
using Discord;
using Discord.Webhook;
using LBPUnion.ProjectLighthouse.Types.Settings;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class WebhookHelper
{
    private static readonly DiscordWebhookClient client = (ServerConfiguration.Instance.DiscordIntegration.DiscordIntegrationEnabled
        ? new DiscordWebhookClient(ServerConfiguration.Instance.DiscordIntegration.Url)
        : null);

    public static readonly Color UnionColor = new(0, 140, 255);

    public static Task SendWebhook(EmbedBuilder builder) => SendWebhook(builder.Build());

    public static async Task SendWebhook(Embed embed)
    {
        if (!ServerConfiguration.Instance.DiscordIntegration.DiscordIntegrationEnabled) return;

        await client.SendMessageAsync
        (
            embeds: new[]
            {
                embed,
            }
        );
    }

    public static Task SendWebhook(string title, string description)
        => SendWebhook
        (
            new EmbedBuilder
            {
                Title = title,
                Description = description,
                Color = UnionColor,
            }
        );
}