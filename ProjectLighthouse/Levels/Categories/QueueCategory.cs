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
    public override string Description { get; set; } = "Your queued levels";
    public override string IconHash { get; set; } = "g820614";

    public override string Endpoint { get; set; } = "queue";

    public override Slot? GetPreviewSlot(Database database, User user)
        => database.QueuedLevels.Include(q => q.Slot).FirstOrDefault(q => q.UserId == user.UserId)?.Slot;
    public override IEnumerable<Slot> GetSlots(Database database, User user, int pageStart, int pageSize)
        => database.QueuedLevels.Include
                (q => q.Slot)
            .Include(q => q.Slot.Location)
            .Select(q => q.Slot)
            .ByGameVersion(GameVersion.LittleBigPlanet3)
            .Skip(Math.Max(0, pageStart - 1))
            .Take(Math.Min(pageSize, 20));

    public override int GetTotalSlots(Database database, User user) => database.QueuedLevels.Count(q => q.UserId == user.UserId);
}