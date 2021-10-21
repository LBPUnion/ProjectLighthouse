using ProjectLighthouse.Serialization;

namespace ProjectLighthouse.Types.Profiles {
    /// <summary>
    /// The location of a slot on a planet.
    /// </summary>
    public class Location {
        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public string Serialize() {
            return LbpSerializer.StringElement("x", this.X) +
                   LbpSerializer.StringElement("y", this.Y);
        }
    }
}