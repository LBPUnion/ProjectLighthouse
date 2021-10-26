using System.IO;
using System.Text;
using LBPUnion.ProjectLighthouse.Helpers;

namespace LBPUnion.ProjectLighthouse.Types {
    // This is all the information I can understand for now. More testing is required.
    // Example data:
    //  - LBP2 digital, with the RPCN username `literally1984`
    //      POST /LITTLEBIGPLANETPS3_XML/login?applicationID=21414&languageID=1&lbp2=1&beta=0&titleID=NPUA80662&country=us
    //      !�0256333||x||��Y literally198bruUP9000-NPUA80662_008D
    //  - LBP2 digital, with the RPCN username `jvyden`
    //      POST /LITTLEBIGPLANETPS3_XML/login?applicationID=21414&languageID=1&lbp2=1&beta=0&titleID=NPUA80662&country=us
    //      !�0220333||/u||=0� jvydebruUP9000-NPUA80662_008D
    // Data is 251 bytes long.
    /// <summary>
    /// The data sent from POST /LOGIN.
    /// </summary>
    public class LoginData {
        public string Username { get; set; }
//        public string GameVersion { get; set; }
//        public int UnknownNumber { get; set; } // Seems to increment by 1000 every login attempt

        public static LoginData CreateFromString(string str) {
            str = str.Replace("\b", ""); // Remove backspace characters

            using MemoryStream ms = new(Encoding.ASCII.GetBytes(str));
            using BinaryReader reader = new(ms);

            LoginData loginData = new();
            
            reader.BaseStream.Position = 80;
            loginData.Username = BinaryHelper.ReadString(reader).Replace("\0", string.Empty);

            return loginData;
        }
    }
}