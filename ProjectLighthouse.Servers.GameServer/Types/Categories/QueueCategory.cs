#nullable enable
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Categories;

public class QueueCategory : CategoryWithUser
{
    public override string Name { get; set; } = "My Queue";
    public override string Description { get; set; } = "Your queued content";
    public override string IconHash { get; set; } = "g820614";
    public override string Endpoint { get; set; } = "queue";
    public override SlotEntity? GetPreviewSlot(DatabaseContext database, UserEntity user)
        => database.QueuedLevels.Where(q => q.UserId == user.UserId)
            .Where(q => q.Slot.Type == SlotType.User && !q.Slot.Hidden && q.Slot.GameVersion <= GameVersion.LittleBigPlanet3)
            .OrderByDescending(q => q.QueuedLevelId)
            .Include(q => q.Slot.Creator)
            .Select(q => q.Slot)
            .ByGameVersion(GameVersion.LittleBigPlanet3, false, false, true)
            .FirstOrDefault();

    public override IQueryable<SlotEntity> GetSlots(DatabaseContext database, UserEntity user, int pageStart, int pageSize)
        => database.QueuedLevels.Where(q => q.UserId == user.UserId)
            .Where(q => q.Slot.Type == SlotType.User && !q.Slot.Hidden && q.Slot.GameVersion <= GameVersion.LittleBigPlanet3)
            .OrderByDescending(q => q.QueuedLevelId)
            .Include(q => q.Slot.Creator)
            .Select(q => q.Slot)
            .ByGameVersion(GameVersion.LittleBigPlanet3, false, false, true)
            .Skip(Math.Max(0, pageStart - 1))
            .Take(Math.Min(pageSize, 20));

    public override int GetTotalSlots(DatabaseContext database, UserEntity user) => database.QueuedLevels.Count(q => q.UserId == user.UserId);
}