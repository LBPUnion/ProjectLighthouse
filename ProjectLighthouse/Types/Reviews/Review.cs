#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Serialization;
using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Reviews
{
    [XmlRoot("review")]
    [XmlType("review")]
    public class Review
    {
        // ReSharper disable once UnusedMember.Global
        [Key]
        public int ReviewId { get; set; }

        [XmlIgnore]
        public int ReviewerId { get; set; }

        [ForeignKey(nameof(ReviewerId))]
        public User Reviewer { get; set; }

        [XmlElement("slot_id")]
        public int SlotId { get; set; }

        [ForeignKey(nameof(SlotId))]
        public Slot Slot { get; set; }

        [XmlElement("timestamp")]
        public long Timestamp { get; set; }

        [XmlElement("labels")]
        public string LabelCollection { get; set; }

        [NotMapped]
        [XmlIgnore]
        public string[] Labels {
            get => this.LabelCollection.Split(",");
            set => this.LabelCollection = string.Join(',', value);
        }

        [XmlElement("deleted")]
        public Boolean Deleted { get; set; }

        [XmlElement("deleted_by")]
        public string DeletedBy { get; set; } // enum ? Needs testing e.g. Moderated/Author/Level Author? etc.

        [XmlElement("text")]
        public string Text { get; set; }

        [NotMapped]
        [XmlElement("thumb")]
        public int Thumb { get; set; } // (unused) -- temp value for getting thumb from review upload body for updating level rating
        
        [NotMapped]
        [XmlElement("thumbsup")]
        public int ThumbsUp { 
            get {
                using Database database = new();

                return database.RatedReviews.Count(r => r.ReviewId == this.ReviewId && r.Thumb == 1);
            } 
        }
        [NotMapped]
        [XmlElement("thumbsdown")]
        public int ThumbsDown { 
            get {
                using Database database = new();

                return database.RatedReviews.Count(r => r.ReviewId == this.ReviewId && r.Thumb == -1);
            } 
        }

        public string Serialize(RatedLevel? yourLevelRating = null, RatedReview? yourRatingStats = null) {
            return this.Serialize("review", yourLevelRating, yourRatingStats);
        }

        public string Serialize(string elementOverride, RatedLevel? yourLevelRating = null, RatedReview? yourRatingStats = null)
        {

            string reviewData = LbpSerializer.TaggedStringElement("slot_id", this.SlotId, "type", this.Slot.Type) +
                                LbpSerializer.StringElement("reviewer", this.Reviewer.Username) +
                                LbpSerializer.StringElement("thumb", yourLevelRating?.Rating) +
                                LbpSerializer.StringElement("timestamp", this.Timestamp) +
                                LbpSerializer.StringElement("labels", this.LabelCollection) +
                                LbpSerializer.StringElement("deleted", this.Deleted) +
                                LbpSerializer.StringElement("deleted_by", this.DeletedBy) +
                                LbpSerializer.StringElement("text", this.Text) +
                                LbpSerializer.StringElement("thumbsup", this.ThumbsUp) +
                                LbpSerializer.StringElement("thumbsdown", this.ThumbsDown) +
                                LbpSerializer.StringElement("yourthumb", yourRatingStats?.Thumb == null ? 0 : yourRatingStats?.Thumb);

            return LbpSerializer.TaggedStringElement(elementOverride, reviewData, "id", this.SlotId + "." + this.Reviewer.Username);
        }
    }

    
}