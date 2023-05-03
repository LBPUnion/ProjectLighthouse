#nullable enable
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Categories;

public class HeartedCategory : CategoryWithUser
{
    public override string Name { get; set; } = "My Hearted Content";
    public override string Description { get; set; } = "Content you've hearted";
    public override string IconHash { get; set; } = "g820611";
    public override string Endpoint { get; set; } = "hearted";
    public override SlotEntity? GetPreviewSlot(DatabaseContext database, UserEntity user) // note: developer slots act up in LBP3 when listed here, so I omitted it
        => database.HeartedLevels.Where(h => h.UserId == user.UserId)
            .Where(h => h.Slot.Type == SlotType.User && !h.Slot.Hidden && h.Slot.GameVersion <= GameVersion.LittleBigPlanet3 && !h.Slot.CrossControllerRequired)
            .OrderByDescending(h => h.HeartedLevelId)
            .Include(h => h.Slot.Creator)
            .Select(h => h.Slot)
            .ByGameVersion(GameVersion.LittleBigPlanet3, false, false, true)
            .FirstOrDefault();

    public override IQueryable<SlotEntity> GetSlots(DatabaseContext database, UserEntity user, int pageStart, int pageSize)
        => database.HeartedLevels.Where(h => h.UserId == user.UserId)
            .Where(h => h.Slot.Type == SlotType.User && !h.Slot.Hidden && h.Slot.GameVersion <= GameVersion.LittleBigPlanet3 && !h.Slot.CrossControllerRequired)
            .OrderByDescending(h => h.HeartedLevelId)
            .Include(h => h.Slot.Creator)
            .Select(h => h.Slot)
            .ByGameVersion(GameVersion.LittleBigPlanet3, false, false, true)
            .Skip(Math.Max(0, pageStart))
            .Take(Math.Min(pageSize, 20));

    public override int GetTotalSlots(DatabaseContext database, UserEntity user) => database.HeartedLevels.Count(h => h.UserId == user.UserId);
}