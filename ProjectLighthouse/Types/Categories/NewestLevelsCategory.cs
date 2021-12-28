using System.Collections.Generic;
using System.Linq;
using LBPUnion.ProjectLighthouse.Types.Levels;

namespace LBPUnion.ProjectLighthouse.Types.Categories
{
    public class NewestLevelsCategory : Category
    {
        public override string Name { get; set; } = "Newest Levels";
        public override string Description { get; set; } = "Levels recently published";
        public override string IconHash { get; set; } = "g820623";
        public override string Endpoint { get; set; } = "newest";
        public override IEnumerable<Slot> Slots(Database database) => database.Slots.OrderByDescending(s => s.FirstUploaded).Take(1);
    }
}