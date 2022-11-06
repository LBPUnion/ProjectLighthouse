#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Levels.Categories;

public class QueueCategory : CategoryWithUser
{
    public override string Name { get; set; } = "My Queue";
    public override string Description { get; set; } = "Your queued content";
    public override string IconHash { get; set; } = "g820614";
    public override string Endpoint { get; set; } = "queue";
    public override Slot? GetPreviewSlot(Database database, User user)
        => database.QueuedLevels.Where(q => q.UserId == user.UserId)
            .Where(q => q.Slot.Type == SlotType.User && !q.Slot.Hidden && q.Slot.GameVersion <= GameVersion.LittleBigPlanet3)
            .OrderByDescending(q => q.QueuedLevelId)
            .Include(q => q.Slot.Creator)
            .Include(q => q.Slot.Location)
            .Select(q => q.Slot)
            .ByGameVersion(GameVersion.LittleBigPlanet3, false, false, true)
            .FirstOrDefault();

    public override IEnumerable<Slot> GetSlots(Database database, User user, int pageStart, int pageSize)
        => database.QueuedLevels.Where(q => q.UserId == user.UserId)
            .Where(q => q.Slot.Type == SlotType.User && !q.Slot.Hidden && q.Slot.GameVersion <= GameVersion.LittleBigPlanet3)
            .OrderByDescending(q => q.QueuedLevelId)
            .Include(q => q.Slot.Creator)
            .Include(q => q.Slot.Location)
            .Select(q => q.Slot)
            .ByGameVersion(GameVersion.LittleBigPlanet3, false, false, true)
            .Skip(Math.Max(0, pageStart - 1))
            .Take(Math.Min(pageSize, 20));

    public override int GetTotalSlots(Database database, User user) => database.QueuedLevels.Count(q => q.UserId == user.UserId);
}