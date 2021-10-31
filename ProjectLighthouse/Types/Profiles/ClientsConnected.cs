using LBPUnion.ProjectLighthouse.Serialization;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Types.Profiles
{
    [Keyless]
    public class ClientsConnected
    {
        public bool Lbp1 { get; set; }
        public bool Lbp2 { get; set; }
        public bool LbpMe { get; set; }
        public bool Lbp3Ps3 { get; set; }
        public bool Lbp3Ps4 { get; set; }

        public string Serialize()
            => LbpSerializer.StringElement
            (
                "clientsConnected",
                LbpSerializer.StringElement("lbp1", this.Lbp1) +
                LbpSerializer.StringElement("lbp2", this.Lbp2) +
                LbpSerializer.StringElement("lbpme", this.LbpMe) +
                LbpSerializer.StringElement("lbp3ps3", this.Lbp3Ps3) +
                LbpSerializer.StringElement("lbp3ps4", this.Lbp3Ps4)
            );
    }
}