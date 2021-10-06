using ProjectLighthouse.Serialization;

namespace ProjectLighthouse {
    public class NewsImage {
        public string Hash { get; set; }
        public string Alignment { get; set; }

        public string Serialize() {
            return LbpSerializer.GetStringElement("image", 
                LbpSerializer.GetStringElement("hash", Hash) +
                LbpSerializer.GetStringElement("alignment", Alignment));
        }
    }
}