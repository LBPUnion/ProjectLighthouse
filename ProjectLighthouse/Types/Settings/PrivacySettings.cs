using LBPUnion.ProjectLighthouse.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Settings;

public class PrivacySettings
{
    public string LevelVisibility { get; set; }
    public string ProfileVisibility { get; set; }

    public string Serialize()
        => LbpSerializer.StringElement
        (
            "privacySettings",
            LbpSerializer.StringElement("levelVisibility", this.LevelVisibility) + LbpSerializer.StringElement("profileVisibility", this.ProfileVisibility)
        );
}