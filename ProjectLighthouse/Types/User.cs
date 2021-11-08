using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types.Profiles;
using LBPUnion.ProjectLighthouse.Types.Settings;

namespace LBPUnion.ProjectLighthouse.Types
{
    public class User
    {
        public readonly ClientsConnected ClientsConnected = new();
        public int UserId { get; set; }
        public string Username { get; set; }
        public string IconHash { get; set; }
        public int Game { get; set; }

        [NotMapped]
        public int Lists { get; set; }

        public string YayHash { get; set; }
        public string BooHash { get; set; }

        /// <summary>
        ///     A user-customizable biography shown on the profile card
        /// </summary>
        public string Biography { get; set; }

        [NotMapped]
        public int ReviewCount => 0;

        [NotMapped]
        public int CommentCount {
            get {
                using Database database = new();
                return database.Comments.Count(c => c.PosterUserId == this.UserId);
            }
        }

        [NotMapped]
        public int PhotosByMeCount {
            get {
                using Database database = new();
                return database.Photos.Count(p => p.CreatorId == this.UserId);
            }
        }

        [NotMapped]
        public int PhotosWithMeCount {
            get {
                using Database database = new();
                return Enumerable.Sum(database.Photos, photo => photo.Subjects.Count(subject => subject.User.UserId == this.UserId));
            }
        }

        public int LocationId { get; set; }

        /// <summary>
        ///     The location of the profile card on the user's earth
        /// </summary>
        [ForeignKey("LocationId")]
        public Location Location { get; set; }

        [NotMapped]
        public int HeartedLevels {
            get {
                using Database database = new();
                return database.HeartedLevels.Count(p => p.UserId == this.UserId);
            }
        }

        [NotMapped]
        public int HeartedUsers {
            get {
                using Database database = new();
                return database.HeartedProfiles.Count(p => p.UserId == this.UserId);
            }
        }

        [NotMapped]
        public int QueuedLevelsCount {
            get {
                using Database database = new();
                return database.QueuedLevels.Count(p => p.UserId == this.UserId);
            }
        }

        public string Pins { get; set; } = "";
        public int StaffChallengeGoldCount { get; set; }
        public int StaffChallengeSilverCount { get; set; }
        public int StaffChallengeBronzeCount { get; set; }

        public string PlanetHash { get; set; } = "";

        public int Hearts {
            get {
                using Database database = new();

                return database.HeartedProfiles.Count(s => s.HeartedUserId == this.UserId);
            }
        }

        public string Serialize()
        {
            string user = LbpSerializer.TaggedStringElement("npHandle", this.Username, "icon", this.IconHash) +
                          LbpSerializer.StringElement("game", this.Game) +
                          this.SerializeSlots() +
                          LbpSerializer.StringElement("lists", this.Lists) +
                          LbpSerializer.StringElement("lists_quota", ServerSettings.ListsQuota) + // technically not a part of the user but LBP expects it
                          LbpSerializer.StringElement("yay2", this.YayHash) +
                          LbpSerializer.StringElement("boo2", this.BooHash) +
                          LbpSerializer.StringElement("biography", this.Biography) +
                          LbpSerializer.StringElement("reviewCount", this.ReviewCount) +
                          LbpSerializer.StringElement("commentCount", this.CommentCount) +
                          LbpSerializer.StringElement("photosByMeCount", this.PhotosByMeCount) +
                          LbpSerializer.StringElement("photosWithMeCount", this.PhotosWithMeCount) +
                          LbpSerializer.StringElement("commentsEnabled", "true") +
                          LbpSerializer.StringElement("location", this.Location.Serialize()) +
                          LbpSerializer.StringElement("favouriteSlotCount", this.HeartedLevels) +
                          LbpSerializer.StringElement("favouriteUserCount", this.HeartedUsers) +
                          LbpSerializer.StringElement("lolcatftwCount", this.QueuedLevelsCount) +
                          LbpSerializer.StringElement("pins", this.Pins) +
                          LbpSerializer.StringElement("staffChallengeGoldCount", this.StaffChallengeGoldCount) +
                          LbpSerializer.StringElement("staffChallengeSilverCount", this.StaffChallengeSilverCount) +
                          LbpSerializer.StringElement("staffChallengeBronzeCount", this.StaffChallengeBronzeCount) +
                          LbpSerializer.StringElement("planets", this.PlanetHash) +
                          LbpSerializer.BlankElement("photos") +
                          LbpSerializer.StringElement("heartCount", Hearts);
            this.ClientsConnected.Serialize();

            return LbpSerializer.TaggedStringElement("user", user, "type", "user");
        }

        #region Slots

        /// <summary>
        ///     The number of used slots on the earth
        /// </summary>
        [NotMapped]
        public int UsedSlots {
            get {
                using Database database = new();
                return database.Slots.Count(s => s.CreatorId == this.UserId);
            }
        }

        /// <summary>
        ///     The number of slots remaining on the earth
        /// </summary>
        public int FreeSlots => ServerSettings.EntitledSlots - this.UsedSlots;

        private static readonly string[] slotTypes =
        {
//            "lbp1",
            "lbp2", "lbp3", "crossControl",
        };

        private string SerializeSlots()
        {
            string slots = string.Empty;

            slots += LbpSerializer.StringElement("lbp1UsedSlots", this.UsedSlots);
            slots += LbpSerializer.StringElement("entitledSlots", ServerSettings.EntitledSlots);
            slots += LbpSerializer.StringElement("freeSlots", this.FreeSlots);

            foreach (string slotType in slotTypes)
            {
                slots += LbpSerializer.StringElement(slotType + "UsedSlots", 0);
                slots += LbpSerializer.StringElement(slotType + "EntitledSlots", ServerSettings.EntitledSlots);
                // ReSharper disable once StringLiteralTypo
                slots += LbpSerializer.StringElement(slotType + slotType == "crossControl" ? "PurchsedSlots" : "PurchasedSlots", 0);
                slots += LbpSerializer.StringElement(slotType + "FreeSlots", this.FreeSlots);
            }
            return slots;

        }

        #endregion Slots

    }
}