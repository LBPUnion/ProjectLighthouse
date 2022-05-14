#nullable enable
namespace LBPUnion.ProjectLighthouse.Types.Settings.ConfigurationCategories;

public class DiscordIntegrationConfiguration
{
    //TODO: integrations should be modular/abstracted away

    public bool DiscordIntegrationEnabled { get; set; }

    public string Url { get; set; } = "";
}