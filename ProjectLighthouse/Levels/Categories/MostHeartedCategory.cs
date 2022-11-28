#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.PlayerData;

namespace LBPUnion.ProjectLighthouse.Levels.Categories;

public class MostHeartedCategory : Category
{
    Random rand = new();
    public override string Name { get; set; } = "Most Hearted";
    public override string Description { get; set; } = "The Most Hearted Content";
    public override string IconHash { get; set; } = "g820607";
    public override string Endpoint { get; set; } = "mostHearted";
    public override Slot? GetPreviewSlot(Database database) => database.Slots.Where(s => s.Type == SlotType.User).AsEnumerable().OrderByDescending(s => s.Hearts).FirstOrDefault();
    public override IEnumerable<Slot> GetSlots
        (Database database, int pageStart, int pageSize)
        => database.Slots.ByGameVersion(GameVersion.LittleBigPlanet3, false, true)
        .AsEnumerable()
            .OrderByDescending(s => s.Hearts)
            .ThenBy(_ => rand.Next())
            .Skip(Math.Max(0, pageStart - 1))
            .Take(Math.Min(pageSize, 20));
    public override int GetTotalSlots(Database database) => database.Slots.Count(s => s.Type == SlotType.User);
}