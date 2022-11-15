#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.PlayerData;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Levels.Categories;

public class LuckyDipCategory : Category
{
    public override string Name { get; set; } = "Lucky Dip";
    public override string Description { get; set; } = "Randomized uploaded content";
    public override string IconHash { get; set; } = "g820605";
    public override string Endpoint { get; set; } = "lbp2luckydip";
    public override Slot? GetPreviewSlot(Database database) => database.Slots.Where(s => s.Type == SlotType.User).OrderByDescending(_ => EF.Functions.Random()).FirstOrDefault();
    public override IEnumerable<Slot> GetSlots
        (Database database, int pageStart, int pageSize)
        => database.Slots.ByGameVersion(GameVersion.LittleBigPlanet3, false, true)
            .OrderByDescending(_ => EF.Functions.Random())
            .Skip(Math.Max(0, pageStart - 1))
            .Take(Math.Min(pageSize, 20));
    public override int GetTotalSlots(Database database) => database.Slots.Count(s => s.Type == SlotType.User);
}