using ProjectLighthouse.Serialization;

namespace ProjectLighthouse.Types {
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
        public int FavouriteSlotCount { get; set; }
        public int FavouriteUserCount { get; set; }
        public int lolcatftwCount { get; set; }
        public string Pins { get; set; }
        public int StaffChallengeGoldCount { get; set; }
        public int StaffChallengeSilverCount { get; set; }
        public int StaffChallengeBronzeCount { get; set; }
        public ClientsConnected ClientsConnected;
        
        #region Slots
        
        public static int EntitledSlots = 20;
        public int UsedSlots { get; set; }
        public int FreeSlots => EntitledSlots - this.UsedSlots;

        private static string[] slotTypes = {
            "lbp1",
            "lbp2",
            "lbp3",
            "crossControl"
        };

        private string SerializeSlots() {
            string slots = string.Empty;
            
            foreach(string s in slotTypes) {
                string slotType = s; // vars in foreach are immutable, define helper var
                
                slots += LbpSerializer.GetStringElement(slotType + "UsedSlots", this.UsedSlots);
                if(slotType == "lbp1") slotType = "";
                slots += LbpSerializer.GetStringElement(slotType + "EntitledSlots", EntitledSlots);
                // ReSharper disable once StringLiteralTypo
                slots += LbpSerializer.GetStringElement(slotType + slotType == "crossControl" ? "PurchsedSlots" : "PurchasedSlots", 0);
                slots += LbpSerializer.GetStringElement(slotType + "FreeSlots", this.FreeSlots);
            }
            return slots;
            
        }
        
        #endregion Slots

        public string Serialize() {
            string user = LbpSerializer.GetTaggedStringElement("npHandle", this.Username, "icon", this.IconHash) +
                          LbpSerializer.GetStringElement("game", this.Game) +
                          this.SerializeSlots() +
                          LbpSerializer.GetStringElement("lists", this.Lists) +
                          LbpSerializer.GetStringElement("lists_quota", ListsQuota) +
                          LbpSerializer.GetStringElement("heartCount", this.HeartCount) +
                          LbpSerializer.GetStringElement("yay2", this.YayHash) +
                          LbpSerializer.GetStringElement("boo2", this.BooHash) +
                          LbpSerializer.GetStringElement("biography", this.Biography) +
                          LbpSerializer.GetStringElement("reviewCount", this.ReviewCount) +
                          LbpSerializer.GetStringElement("commentCount", this.CommentCount) +
                          LbpSerializer.GetStringElement("photosByMeCount", this.PhotosByMeCount) +
                          LbpSerializer.GetStringElement("photosWithMeCount", this.PhotosWithMeCount) +
                          LbpSerializer.GetStringElement("commentsEnabled", this.CommentsEnabled) +
                          this.Location.Serialize() +
                          LbpSerializer.GetStringElement("favouriteSlotCount", this.FavouriteSlotCount) +
                          LbpSerializer.GetStringElement("favouriteUserCount", this.FavouriteUserCount) +
                          LbpSerializer.GetStringElement("lolcatftwCount", this.lolcatftwCount) +
                          LbpSerializer.GetStringElement("pins", this.Pins) +
                          LbpSerializer.GetStringElement("staffChallengeGoldCount", this.StaffChallengeGoldCount) +
                          LbpSerializer.GetStringElement("staffChallengeSilverCount", this.StaffChallengeSilverCount) +
                          LbpSerializer.GetStringElement("staffChallengeBronzeCount", this.StaffChallengeBronzeCount) +
                          LbpSerializer.GetStringElement("photos", "") +
                          this.ClientsConnected.Serialize();
            
            return LbpSerializer.GetTaggedStringElement("user", user, "type", "user");
        }
    }
}