using ProjectLighthouse.Serialization;

namespace ProjectLighthouse.Types {
    /// <summary>
    /// The location of a slot on a planet.
    /// </summary>
    public class Location {
        public int X;
        public int Y;

        public string Serialize() {
            return LbpSerializer.StringElement("x", this.X) +
                   LbpSerializer.StringElement("x", this.Y);
        }
    }
}