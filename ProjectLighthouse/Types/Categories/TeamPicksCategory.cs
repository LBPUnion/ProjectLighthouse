#nullable enable
using System.Linq;
using LBPUnion.ProjectLighthouse.Types.Levels;

namespace LBPUnion.ProjectLighthouse.Types.Categories
{
    public class TeamPicksCategory : Category
    {
        public override string Name { get; set; } = "Team Picks";
        public override string Description { get; set; } = "Levels given awards by your instance admin";
        public override string IconHash { get; set; } = "g820626";
        public override string Endpoint { get; set; } = "team_picks";
        public override Slot? GetPreviewSlot(Database database) => database.Slots.OrderByDescending(s => s.FirstUploaded).FirstOrDefault(s => s.TeamPick);
    }
}