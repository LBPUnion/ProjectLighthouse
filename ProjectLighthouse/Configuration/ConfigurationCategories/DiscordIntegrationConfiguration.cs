#nullable enable
namespace LBPUnion.ProjectLighthouse.Configuration.ConfigurationCategories;

public class DiscordIntegrationConfiguration
{
    //TODO: integrations should be modular/abstracted away

    public bool DiscordIntegrationEnabled { get; set; }

    public string Url { get; set; } = "";

    public string ModerationUrl { get; set; } = "";
}