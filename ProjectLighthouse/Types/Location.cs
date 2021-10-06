using ProjectLighthouse.Serialization;

namespace ProjectLighthouse {
    public class Location {
        public int X;
        public int Y;

        public string Serialize() {
            return LbpSerializer.GetStringElement("x", this.X) +
                   LbpSerializer.GetStringElement("Y", this.Y);
        }
    }
}