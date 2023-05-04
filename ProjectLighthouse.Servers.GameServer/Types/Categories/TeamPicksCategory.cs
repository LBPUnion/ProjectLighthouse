#nullable enable
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Users;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Categories;

public class TeamPicksCategory : Category
{
    public override string Name { get; set; } = "Team Picks";
    public override string Description { get; set; } = "Community Team Picks";
    public override string IconHash { get; set; } = "g820626";
    public override string Endpoint { get; set; } = "team_picks";
    public override SlotEntity? GetPreviewSlot(DatabaseContext database) => database.Slots.OrderByDescending(s => s.FirstUploaded).FirstOrDefault(s => s.TeamPick && !s.CrossControllerRequired);
    public override IQueryable<SlotEntity> GetSlots
        (DatabaseContext database, int pageStart, int pageSize)
        => database.Slots.ByGameVersion(GameVersion.LittleBigPlanet3, false, true)
            .OrderByDescending(s => s.FirstUploaded)
            .Where(s => s.TeamPick && !s.CrossControllerRequired)
            .Skip(Math.Max(0, pageStart - 1))
            .Take(Math.Min(pageSize, 20));
    public override int GetTotalSlots(DatabaseContext database) => database.Slots.Count(s => s.TeamPick);
}