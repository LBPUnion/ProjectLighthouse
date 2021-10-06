using System.Collections.Generic;
using ProjectLighthouse.Serialization;

namespace ProjectLighthouse.Types {
    public class LoginResult {
        public string AuthTicket { get; set; }
        public string LbpEnvVer { get; set; }

        public string Serialize() {
            return LbpSerializer.GetElements(
                new KeyValuePair<string, object>("authTicket", this.AuthTicket),
                new KeyValuePair<string, object>("lbpEnvVer", this.LbpEnvVer)
            );
        }
    }
}