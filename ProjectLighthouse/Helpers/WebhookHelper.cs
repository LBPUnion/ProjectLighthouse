using System;
using System.Threading.Tasks;
using Discord;
using Discord.Webhook;
using LBPUnion.ProjectLighthouse.Configuration;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class WebhookHelper
{
    /// <summary>
    /// The destination of the webhook post.
    /// </summary>
    public enum WebhookDestination : byte
    {
        /// <summary>
        /// A channel intended for public viewing; where new levels and photos are sent.
        /// </summary>
        Public,
        /// <summary>
        /// A channel intended for moderators; where grief reports are sent.
        /// </summary>
        Moderation,
    }
    
    private static readonly DiscordWebhookClient publicClient = (ServerConfiguration.Instance.DiscordIntegration.DiscordIntegrationEnabled
        ? new DiscordWebhookClient(ServerConfiguration.Instance.DiscordIntegration.Url)
        : null);

    private static readonly DiscordWebhookClient moderationClient = (ServerConfiguration.Instance.DiscordIntegration.DiscordIntegrationEnabled
        ? new DiscordWebhookClient(ServerConfiguration.Instance.DiscordIntegration.ModerationUrl)
        : null);

    public static readonly Color UnionColor = new(0, 140, 255);

    public static Task SendWebhook(EmbedBuilder builder, WebhookDestination dest = WebhookDestination.Public)
        => SendWebhook(builder.Build(), dest);

    public static async Task SendWebhook(Embed embed, WebhookDestination dest = WebhookDestination.Public)
    {
        if (!ServerConfiguration.Instance.DiscordIntegration.DiscordIntegrationEnabled) return;
        
        DiscordWebhookClient client = dest switch
        {
            WebhookDestination.Public => publicClient,
            WebhookDestination.Moderation => moderationClient,
            _ => throw new ArgumentOutOfRangeException(nameof(dest), dest, null),
        };

        await client.SendMessageAsync
        (
            embeds: new[]
            {
                embed,
            }
        );
    }

    public static Task SendWebhook(string title, string description, WebhookDestination dest = WebhookDestination.Public)
        => SendWebhook
        (
            new EmbedBuilder
            {
                Title = title,
                Description = description,
                Color = UnionColor,
            },
            dest
        );
}