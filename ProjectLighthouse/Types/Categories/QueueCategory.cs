#nullable enable
using System.Collections.Generic;
using System.Linq;
using LBPUnion.ProjectLighthouse.Types.Levels;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Types.Categories
{
    public class QueueCategory : CategoryWithUser
    {
        public override string Name { get; set; } = "My Queue";
        public override string Description { get; set; } = "Your queued levels";
        public override string IconHash { get; set; } = "g820614";

        public override string Endpoint { get; set; } = "queue";
        public override IEnumerable<Slot> GetSlots(Database database, int pageStart, int pageSize) => new List<Slot>();

        public override Slot? GetPreviewSlot(Database database, User user)
            => database.QueuedLevels.Include(q => q.Slot).FirstOrDefault(q => q.UserId == user.UserId)?.Slot;

        public override int GetTotalSlots(Database database, User user) => database.QueuedLevels.Count(q => q.UserId == user.UserId);
    }
}