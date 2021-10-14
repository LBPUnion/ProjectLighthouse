using System.IO;
using System.Text;

namespace ProjectLighthouse.Types {
    // This is all the information I can understand for now. More testing is required.
    // Example data:
    //  - LBP2 digital, with the RPCN username `literally1984`
    //      POST /LITTLEBIGPLANETPS3_XML/login?applicationID=21414&languageID=1&lbp2=1&beta=0&titleID=NPUA80662&country=us
    //      !�0256333||x||��Y literally198bruUP9000-NPUA80662_008D
    //  - LBP2 digital, with the RPCN username `jvyden`
    //      POST /LITTLEBIGPLANETPS3_XML/login?applicationID=21414&languageID=1&lbp2=1&beta=0&titleID=NPUA80662&country=us
    //      !�0220333||/u||=0� jvydebruUP9000-NPUA80662_008D
    /// <summary>
    /// The data sent from POST /LOGIN.
    /// </summary>
    public class LoginData {
        public string Username { get; set; } // Cut off by one for some strange reason
        public string GameVersion { get; set; }
        public int UnknownNumber { get; set; } // Seems to increment by 1000 every login attempt

        public static LoginData CreateFromString(string str) {
            using MemoryStream ms = new(Encoding.ASCII.GetBytes(str));
            using BinaryReader reader = new(ms);

            LoginData loginData = new();

            reader.ReadBytes(4); // Perhaps a header of sorts?
            
            string number = Encoding.ASCII.GetString(reader.ReadBytes(7)); // Number is stored as text for some reason...
            loginData.UnknownNumber = int.Parse(number);

            reader.ReadBytes(10); // No clue what this is.

            string end = Encoding.ASCII.GetString(reader.ReadBytes(int.MaxValue)); // ReadToEnd 2: Electric Boogaloo
            string[] split = end.Split("bru"); // No idea what it means, but it seems to split the gameversion and username apart
            
            loginData.Username = split[0];
            loginData.GameVersion = split[1];

            return loginData;
        }
    }
}