using System.Collections.Generic;
using System.Xml.Serialization;
using ProjectLighthouse.Serialization;

namespace ProjectLighthouse {
    public class LoginResult {
        public string AuthTicket { get; set; }
        public string LbpEnvVer { get; set; }

        public string Serialize() {
            return LbpSerializer.GetElements(
                new KeyValuePair<string, object>("authTicket", AuthTicket),
                new KeyValuePair<string, object>("lbpEnvVer", LbpEnvVer)
            );
        }
    }
}