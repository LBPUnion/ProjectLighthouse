using System;
using System.Threading.Tasks;
using Discord;
using Discord.Webhook;
using LBPUnion.ProjectLighthouse.Configuration;
using SixLabors.ImageSharp.PixelFormats;
using Color = SixLabors.ImageSharp.Color;
using DiscordColor = Discord.Color;

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
        /// <summary>
        /// A channel intended for public viewing; specifically for announcing user registrations
        /// </summary>
        Registration,
    }

    private static bool isDestinationValid(WebhookDestination dest)
    {
        if (!DiscordConfiguration.Instance.DiscordIntegrationEnabled) return false;
        string url = dest switch
        {
            WebhookDestination.Public => DiscordConfiguration.Instance.PublicUrl,
            WebhookDestination.Moderation => DiscordConfiguration.Instance.ModerationUrl,
            WebhookDestination.Registration => DiscordConfiguration.Instance.RegistrationUrl,
            _ => throw new ArgumentOutOfRangeException(nameof(dest), dest, null),
        };
        return !string.IsNullOrWhiteSpace(url);
    }

    private static readonly DiscordWebhookClient publicClient = isDestinationValid(WebhookDestination.Public)
        ? new DiscordWebhookClient(DiscordConfiguration.Instance.PublicUrl)
        : null;

    private static readonly DiscordWebhookClient moderationClient = isDestinationValid(WebhookDestination.Moderation)
        ? new DiscordWebhookClient(DiscordConfiguration.Instance.ModerationUrl)
        : null;

    private static readonly DiscordWebhookClient registrationClient = isDestinationValid(WebhookDestination.Registration)
        ? new DiscordWebhookClient(DiscordConfiguration.Instance.RegistrationUrl)
        : null;

    public static Task SendWebhook(EmbedBuilder builder, WebhookDestination dest = WebhookDestination.Public)
        => SendWebhook(builder.Build(), dest);

    public static async Task SendWebhook(Embed embed, WebhookDestination dest = WebhookDestination.Public)
    {
        if (!DiscordConfiguration.Instance.DiscordIntegrationEnabled) return;
        
        DiscordWebhookClient client = dest switch
        {
            WebhookDestination.Public => publicClient,
            WebhookDestination.Moderation => moderationClient,
            WebhookDestination.Registration => registrationClient,
            _ => throw new ArgumentOutOfRangeException(nameof(dest), dest, null),
        };
        if (client == null) return;

        await client.SendMessageAsync
        (
            embeds: new[]
            {
                embed,
            }
        );
    }

    public static DiscordColor GetEmbedColor()
    {
        Color embedColor = Color.ParseHex(DiscordConfiguration.Instance.EmbedColor);
        Rgb24 pixel = embedColor.ToPixel<Rgb24>();
        return new DiscordColor(pixel.R, pixel.G, pixel.B);
    }

    public static Task SendWebhook(string title, string description, WebhookDestination dest = WebhookDestination.Public)
        => SendWebhook
        (
            new EmbedBuilder
            {
                Title = title,
                Description = description,
                Color = GetEmbedColor(),
            },
            dest
        );
}