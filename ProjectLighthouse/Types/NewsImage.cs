using ProjectLighthouse.Serialization;

namespace ProjectLighthouse.Types {
    public class NewsImage {
        public string Hash { get; set; }
        public string Alignment { get; set; }

        public string Serialize() {
            return LbpSerializer.GetStringElement("image", 
                LbpSerializer.GetStringElement("hash", this.Hash) +
                LbpSerializer.GetStringElement("alignment", this.Alignment));
        }
    }
}