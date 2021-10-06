using ProjectLighthouse.Serialization;

namespace ProjectLighthouse.Types {
    public class Location {
        public int X;
        public int Y;

        public string Serialize() {
            return LbpSerializer.StringElement("x", this.X) +
                   LbpSerializer.StringElement("Y", this.Y);
        }
    }
}