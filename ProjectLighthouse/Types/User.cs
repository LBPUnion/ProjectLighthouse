using ProjectLighthouse.Serialization;
using ProjectLighthouse.Types;

namespace ProjectLighthouse {
    public class User {
        public string Username { get; set; }
        public string IconHash { get; set; }
        public int Game { get; set; }
        public int Lists { get; set; }
        public static int ListsQuota = 20;
        public int HeartCount { get; set; }
        public string YayHash { get; set; }
        public string BooHash { get; set; }
        public string Biography { get; set; }
        public int ReviewCount { get; set; }
        public int CommentCount { get; set; }
        public int PhotosByMeCount { get; set; }
        public int PhotosWithMeCount { get; set; }
        public bool CommentsEnabled { get; set; }
        public Location Location { get; set; }
        public string Pins { get; set; }
        public int StaffChallengeGoldCount { get; set; }
        public int StaffChallengeSilverCount { get; set; }
        public int StaffChallengeBronzeCount { get; set; }
        public ClientsConnected ClientsConnected;
        
        #region Slots
        
        public static int EntitledSlots = 20;
        public int UsedSlots { get; set; }
        public int FreeSlots => EntitledSlots - UsedSlots;

        private static string[] slotTypes = {
            "lbp1",
            "lbp2",
            "lbp3",
            "crossControl"
        };

        private string SerializeSlots() {
            string slots = string.Empty;
            
            foreach(string slotType in slotTypes) {
                slots += LbpSerializer.GetStringElement(slotType + "UsedSlots", UsedSlots);
                slots += LbpSerializer.GetStringElement(slotType + "EntitledSlots", EntitledSlots);
                // ReSharper disable once StringLiteralTypo
                slots += LbpSerializer.GetStringElement(slotType + slotType == "crossControl" ? "PurchsedSlots" : "PurchasedSlots", 0);
                slots += LbpSerializer.GetStringElement(slotType + "FreeSlots", FreeSlots);
            }
            return slots;
            
        }
        
        #endregion Slots

        public string Serialize() {
            string user = LbpSerializer.GetTaggedStringElement("npHandle", Username, "icon", IconHash) +
                          this.SerializeSlots() +
                          LbpSerializer.GetStringElement("game", Game);
            
            return LbpSerializer.GetTaggedStringElement("user", user, "type", "user");
        }
    }
}