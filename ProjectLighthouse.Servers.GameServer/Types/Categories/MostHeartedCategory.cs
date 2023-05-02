#nullable enable
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Misc;
using LBPUnion.ProjectLighthouse.Types.Users;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Categories;

public class MostHeartedCategory : Category
{
    public override string Name { get; set; } = "Most Hearted";
    public override string Description { get; set; } = "The Most Hearted Content";
    public override string IconHash { get; set; } = "g820607";
    public override string Endpoint { get; set; } = "mostHearted";

    public override SlotEntity? GetPreviewSlot(DatabaseContext database) =>
        database.Slots.Where(s => s.Type == SlotType.User && !s.CrossControllerRequired)
            .Select(s => new SlotMetadata
            {
                Slot = s,
                Hearts = database.HeartedLevels.Count(r => r.SlotId == s.SlotId),
            })
            .OrderByDescending(s => s.Hearts)
            .Select(s => s.Slot)
            .FirstOrDefault();

    public override IEnumerable<SlotEntity> GetSlots(DatabaseContext database, int pageStart, int pageSize) =>
        database.Slots.ByGameVersion(GameVersion.LittleBigPlanet3, false, true)
            .Where(s => !s.CrossControllerRequired)
            .Select(s => new SlotMetadata
            {
                Slot = s,
                Hearts = database.HeartedLevels.Count(r => r.SlotId == s.SlotId),
            })
            .OrderByDescending(s => s.Hearts)
            .Select(s => s.Slot)
            .Skip(Math.Max(0, pageStart - 1))
            .Take(Math.Min(pageSize, 20));
    public override int GetTotalSlots(DatabaseContext database) => database.Slots.Count(s => s.Type == SlotType.User);
}