#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Levels.Categories;

public class HeartedCategory : CategoryWithUser
{
    public override string Name { get; set; } = "My Hearted Content";
    public override string Description { get; set; } = "Content you've hearted";
    public override string IconHash { get; set; } = "g820611";
    public override string Endpoint { get; set; } = "hearted";
    public override Slot? GetPreviewSlot(Database database, User user) // note: developer slots act up in LBP3 when listed here, so I omitted it
        => database.HeartedLevels.Where(h => h.UserId == user.UserId)
            .Where(h => h.Slot.Type == SlotType.User && !h.Slot.Hidden && h.Slot.GameVersion <= GameVersion.LittleBigPlanet3)
            .OrderByDescending(h => h.HeartedLevelId)
            .Include(h => h.Slot.Creator)
            .Include(h => h.Slot.Location)
            .Select(h => h.Slot)
            .ByGameVersion(GameVersion.LittleBigPlanet3, false, false, true)
            .FirstOrDefault();

    public override IEnumerable<Slot> GetSlots(Database database, User user, int pageStart, int pageSize)
        => database.HeartedLevels.Where(h => h.UserId == user.UserId)
            .Where(h => h.Slot.Type == SlotType.User && !h.Slot.Hidden && h.Slot.GameVersion <= GameVersion.LittleBigPlanet3)
            .OrderByDescending(h => h.HeartedLevelId)
            .Include(h => h.Slot.Creator)
            .Include(h => h.Slot.Location)
            .Select(h => h.Slot)
            .ByGameVersion(GameVersion.LittleBigPlanet3, false, false, true)
            .Skip(Math.Max(0, pageStart))
            .Take(Math.Min(pageSize, 20));

    public override int GetTotalSlots(Database database, User user) => database.HeartedLevels.Count(h => h.UserId == user.UserId);
}