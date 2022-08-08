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
    public override string Name { get; set; } = "My Hearted Levels";
    public override string Description { get; set; } = "Levels you've hearted in the past";
    public override string IconHash { get; set; } = "g820607";
    public override string Endpoint { get; set; } = "hearted";
    public override Slot? GetPreviewSlot(Database database, User user) => database.HeartedLevels.OrderByDescending(h => h.HeartedLevelId).LastOrDefault(h => h.UserId == user.UserId)?.Slot;
    public override int GetTotalSlots(Database database, User user) => database.HeartedLevels.Count(h => h.UserId == user.UserId);
    public override IEnumerable<Slot> GetSlots(Database database, User user, int pageStart, int pageSize)
        => database.HeartedLevels.Where(h => h.UserId == user.UserId)
            .OrderByDescending(h => h.HeartedLevelId)
            .Include(h => h.Slot)
            .Select(h => h.Slot)
            .ByGameVersion(GameVersion.LittleBigPlanet3)
            .Skip(pageStart)
            .Take(Math.Min(pageSize, 20));
}