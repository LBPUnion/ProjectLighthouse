#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using LBPUnion.ProjectLighthouse.Entities.Level;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Users;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Levels.Categories;

public class MostHeartedCategory : Category
{
    public override string Name { get; set; } = "Most Hearted";
    public override string Description { get; set; } = "The Most Hearted Content";
    public override string IconHash { get; set; } = "g820607";
    public override string Endpoint { get; set; } = "mostHearted";
    public override Slot? GetPreviewSlot(Database database) => database.Slots.Where(s => s.Type == SlotType.User).AsEnumerable().MaxBy(s => s.Hearts);
    public override IEnumerable<Slot> GetSlots
        (Database database, int pageStart, int pageSize)
        => database.Slots.ByGameVersion(GameVersion.LittleBigPlanet3, false, true)
        .AsEnumerable()
            .OrderByDescending(s => s.Hearts)
            .ThenBy(_ => EF.Functions.Random())
            .Skip(Math.Max(0, pageStart - 1))
            .Take(Math.Min(pageSize, 20));
    public override int GetTotalSlots(Database database) => database.Slots.Count(s => s.Type == SlotType.User);
}