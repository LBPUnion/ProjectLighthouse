#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using LBPUnion.ProjectLighthouse.Helpers.Extensions;
using LBPUnion.ProjectLighthouse.Types.Levels;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Types.Categories;

public class HeartedCategory : CategoryWithUser
{
    public override string Name { get; set; } = "My Hearted Levels";
    public override string Description { get; set; } = "Levels you've hearted in the past";
    public override string IconHash { get; set; } = "g820607";
    public override string Endpoint { get; set; } = "hearted";
    public override Slot? GetPreviewSlot(Database database, User user) => database.HeartedLevels.FirstOrDefault(h => h.UserId == user.UserId)?.Slot;
    public override int GetTotalSlots(Database database, User user) => database.HeartedLevels.Count(h => h.UserId == user.UserId);
    public override IEnumerable<Slot> GetSlots(Database database, User user, int pageStart, int pageSize)
        => database.HeartedLevels.Where(h => h.UserId == user.UserId)
            .Include(h => h.Slot)
            .Select(h => h.Slot)
            .ByGameVersion(GameVersion.LittleBigPlanet3)
            .Skip(pageStart)
            .Take(Math.Min(pageSize, 20));
}