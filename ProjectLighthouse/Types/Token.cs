using Microsoft.EntityFrameworkCore;

namespace ProjectLighthouse.Types {
    [Keyless]
    public class Token {
        public int UserId { get; set; }
        public string MMAuth { get; set; }
    }
}