#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.PlayerData;

namespace LBPUnion.ProjectLighthouse.Levels.Categories;

public class NewestLevelsCategory : Category
{
    public override string Name { get; set; } = "Newest Levels";
    public override string Description { get; set; } = "Levels recently published";
    public override string IconHash { get; set; } = "g820623";
    public override string Endpoint { get; set; } = "newest";
    public override Slot? GetPreviewSlot(Database database) => database.Slots.Where(s => s.Type == SlotType.User).OrderByDescending(s => s.FirstUploaded).FirstOrDefault();
    public override IEnumerable<Slot> GetSlots
        (Database database, int pageStart, int pageSize)
        => database.Slots.ByGameVersion(GameVersion.LittleBigPlanet3, false, true)
            .OrderByDescending(s => s.FirstUploaded)
            .Skip(pageStart - 1)
            .Take(Math.Min(pageSize, 20));
    public override int GetTotalSlots(Database database) => database.Slots.Count(s => s.Type == SlotType.User);
}