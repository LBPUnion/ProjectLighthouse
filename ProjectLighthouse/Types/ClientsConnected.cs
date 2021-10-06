using ProjectLighthouse.Serialization;

namespace ProjectLighthouse.Types {
    public class ClientsConnected {
        public bool Lbp1 { get; set; }
        public bool Lbp2 { get; set; }
        public bool LbpMe { get; set; }
        public bool Lbp3Ps3 { get; set; }
        public bool Lbp3Ps4 { get; set; }

        public string Serialize() {
            return LbpSerializer.GetStringElement("clientsConnected", 
                LbpSerializer.GetStringElement("lbp1", Lbp1) +
                LbpSerializer.GetStringElement("lbp2", Lbp2) +
                LbpSerializer.GetStringElement("lbpme", LbpMe) +
                LbpSerializer.GetStringElement("lbp3ps3", Lbp3Ps3) +
                LbpSerializer.GetStringElement("lbp3ps4", Lbp3Ps4));
        }
    }
}