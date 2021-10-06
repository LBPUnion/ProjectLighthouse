using Microsoft.EntityFrameworkCore;
using ProjectLighthouse.Serialization;

namespace ProjectLighthouse.Types {
    [Keyless]
    public class ClientsConnected {
        public bool Lbp1 { get; set; }
        public bool Lbp2 { get; set; }
        public bool LbpMe { get; set; }
        public bool Lbp3Ps3 { get; set; }
        public bool Lbp3Ps4 { get; set; }

        public string Serialize() {
            return LbpSerializer.StringElement("clientsConnected", 
                LbpSerializer.StringElement("lbp1", Lbp1) +
                LbpSerializer.StringElement("lbp2", Lbp2) +
                LbpSerializer.StringElement("lbpme", LbpMe) +
                LbpSerializer.StringElement("lbp3ps3", Lbp3Ps3) +
                LbpSerializer.StringElement("lbp3ps4", Lbp3Ps4));
        }
    }
}