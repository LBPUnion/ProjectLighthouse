using ProjectLighthouse.Serialization;

namespace ProjectLighthouse.Types {
    public class PrivacySettings {
        public string LevelVisibility { get; set; }
        public string ProfileVisibility { get; set; }

        public string Serialize() {
            return LbpSerializer.StringElement("privacySettings",
                LbpSerializer.StringElement("levelVisibility", LevelVisibility) +
                LbpSerializer.StringElement("profileVisibility", ProfileVisibility)
            );
        }
    }
}