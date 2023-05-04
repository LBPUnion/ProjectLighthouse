#nullable enable
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Misc;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Categories;

public class HighestRatedCategory : Category
{
    public override string Name { get; set; } = "Highest Rated";
    public override string Description { get; set; } = "Community Highest Rated content";
    public override string IconHash { get; set; } = "g820603";
    public override string Endpoint { get; set; } = "thumbs";
    public override SlotEntity? GetPreviewSlot(DatabaseContext database) =>
        database.Slots.Where(s => s.Type == SlotType.User && !s.CrossControllerRequired)
            .Select(s => new SlotMetadata
            {
                Slot = s,
                ThumbsUp = database.RatedLevels.Count(r => r.SlotId == s.SlotId && r.Rating == 1),
            })
            .OrderByDescending(s => s.ThumbsUp)
            .Select(s => s.Slot)
            .FirstOrDefault();

    public override IEnumerable<SlotEntity> GetSlots(DatabaseContext database, int pageStart, int pageSize) =>
        database.Slots.ByGameVersion(GameVersion.LittleBigPlanet3, false, true)
            .Where(s => !s.CrossControllerRequired)
            .Select(s => new SlotMetadata
            {
                Slot = s,
                ThumbsUp = database.RatedLevels.Count(r => r.SlotId == s.SlotId && r.Rating == 1),
            })
            .OrderByDescending(s => s.ThumbsUp)
            .ThenBy(_ => EF.Functions.Random())
            .Select(s => s.Slot)
            .Skip(Math.Max(0, pageStart - 1))
            .Take(Math.Min(pageSize, 20));
    public override int GetTotalSlots(DatabaseContext database) => database.Slots.Count(s => s.Type == SlotType.User);
}