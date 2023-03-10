using YamlDotNet.Serialization;

namespace LBPUnion.ProjectLighthouse.Configuration;

public class DiscordConfiguration : ConfigurationBase<DiscordConfiguration>
{
    // HEY, YOU!
    // THIS VALUE MUST BE INCREMENTED FOR EVERY CONFIG CHANGE!
    //
    // This is so Lighthouse can properly identify outdated configurations and update them with newer settings accordingly.
    // If you are modifying anything here, this value MUST be incremented.
    // Thanks for listening~
    public override int ConfigVersion { get; set; } = 1;

    public override string ConfigName { get; set; } = "discord.yml";

    public override bool NeedsConfiguration { get; set; } = false;

    // TODO integrations should be more modular

    public bool DiscordIntegrationEnabled { get; set; } = false;

    public string EmbedColor { get; set; } = "#008CFF";

    public string PublicUrl { get; set; } = "";

    public string ModerationUrl { get; set; } = "";

    public string RegistrationUrl { get; set; } = "";

    public string RegistrationAnnouncement { get; set; } = "%user just connected to %instance for the first time using %platform!";

    public override ConfigurationBase<DiscordConfiguration> Deserialize(IDeserializer deserializer, string text) => deserializer.Deserialize<DiscordConfiguration>(text);
}