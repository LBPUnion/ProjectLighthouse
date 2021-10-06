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
        
        /// <summary>
        /// A user-customizable biography shown on the profile card
        /// </summary>
        public string Biography { get; set; }
        public int ReviewCount { get; set; }
        public int CommentCount { get; set; }
        public int PhotosByMeCount { get; set; }
        public int PhotosWithMeCount { get; set; }
        public bool CommentsEnabled { get; set; }
        
        /// <summary>
        /// The location of the profile card on the user's earth
        /// </summary>
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
        
        /// <summary>
        /// The maximum amount of slots allowed on the earth
        /// </summary>
        public static int EntitledSlots = 20;
        
        /// <summary>
        /// The number of used slots on the earth
        /// </summary>
        public int UsedSlots { get; set; }
        
        /// <summary>
        /// The number of slots remaining on the earth
        /// </summary>
        public int FreeSlots => EntitledSlots - this.UsedSlots;

        private static string[] slotTypes = {
//            "lbp1",
            "lbp2",
            "lbp3",
            "crossControl"
        };

        private string SerializeSlots() {
            string slots = string.Empty;

            slots += LbpSerializer.StringElement("lbp1UsedSlots", this.UsedSlots);
            slots += LbpSerializer.StringElement("entitledSlots", EntitledSlots);
            slots += LbpSerializer.StringElement("freeSlots", this.FreeSlots);

            foreach(string slotType in slotTypes) {
                slots += LbpSerializer.StringElement(slotType + "UsedSlots", this.UsedSlots);
                slots += LbpSerializer.StringElement(slotType + "EntitledSlots", EntitledSlots);
                // ReSharper disable once StringLiteralTypo
                slots += LbpSerializer.StringElement(slotType + slotType == "crossControl" ? "PurchsedSlots" : "PurchasedSlots", 0);
                slots += LbpSerializer.StringElement(slotType + "FreeSlots", this.FreeSlots);
            }
            return slots;
            
        }
        
        #endregion Slots

        public string Serialize() {
            string user = LbpSerializer.TaggedStringElement("npHandle", this.Username, "icon", this.IconHash) +
                          LbpSerializer.StringElement("game", this.Game) +
                          this.SerializeSlots() +
                          LbpSerializer.StringElement("lists", this.Lists) +
                          LbpSerializer.StringElement("lists_quota", ListsQuota) +
                          LbpSerializer.StringElement("heartCount", this.HeartCount) +
                          LbpSerializer.StringElement("yay2", this.YayHash) +
                          LbpSerializer.StringElement("boo2", this.BooHash) +
                          LbpSerializer.StringElement("biography", this.Biography) +
                          LbpSerializer.StringElement("reviewCount", this.ReviewCount) +
                          LbpSerializer.StringElement("commentCount", this.CommentCount) +
                          LbpSerializer.StringElement("photosByMeCount", this.PhotosByMeCount) +
                          LbpSerializer.StringElement("photosWithMeCount", this.PhotosWithMeCount) +
                          LbpSerializer.StringElement("commentsEnabled", this.CommentsEnabled) +
                          this.Location.Serialize() +
                          LbpSerializer.StringElement("favouriteSlotCount", this.FavouriteSlotCount) +
                          LbpSerializer.StringElement("favouriteUserCount", this.FavouriteUserCount) +
                          LbpSerializer.StringElement("lolcatftwCount", this.lolcatftwCount) +
                          LbpSerializer.StringElement("pins", this.Pins) +
                          LbpSerializer.StringElement("staffChallengeGoldCount", this.StaffChallengeGoldCount) +
                          LbpSerializer.StringElement("staffChallengeSilverCount", this.StaffChallengeSilverCount) +
                          LbpSerializer.StringElement("staffChallengeBronzeCount", this.StaffChallengeBronzeCount) +
                          LbpSerializer.BlankElement("photos") +
                          this.ClientsConnected.Serialize();
            
            return LbpSerializer.TaggedStringElement("user", user, "type", "user");
        }
    }
}