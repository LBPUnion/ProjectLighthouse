#nullable enable
using System.Linq;
using LBPUnion.ProjectLighthouse.Types.Levels;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Types.Categories
{
    public class QueueCategory : Category
    {
        private User user;

        public QueueCategory(User user)
        {
            this.user = user;
        }

        public override string Name { get; set; } = "My Queue";
        public override string Description { get; set; } = "Your queued levels";
        public override string IconHash { get; set; } = "g820614";

        public override string Endpoint {
            get => $"queue/{this.user.UserId}";
            set {
                // cry about it, i don't care
            }
        }

        public override Slot? GetPreviewSlot(Database database)
            => database.QueuedLevels.Include(q => q.Slot).FirstOrDefault(q => q.UserId == this.user.UserId)?.Slot;

        public override int GetTotalSlots(Database database) => database.QueuedLevels.Count(q => q.UserId == this.user.UserId);
    }
}