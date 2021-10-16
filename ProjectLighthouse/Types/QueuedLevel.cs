using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace ProjectLighthouse.Types {
    public class QueuedLevel {
        [Key] public int QueuedLevelId { get; set; }
        
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
        
        public int SlotId { get; set; }

        [ForeignKey(nameof(SlotId))]
        public Slot Slot { get; set; }
    }
}