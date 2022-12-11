#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.PlayerData;
using YamlDotNet.Core.Tokens;

namespace LBPUnion.ProjectLighthouse.Levels.Categories;

public class HighestRatedCategory : Category
{
    Random rand = new();
    public override string Name { get; set; } = "Highest Rated";
    public override string Description { get; set; } = "Community Highest Rated content";
    public override string IconHash { get; set; } = "g820603";
    public override string Endpoint { get; set; } = "thumbs";
    public override Slot? GetPreviewSlot(Database database) => database.Slots.Where(s => s.Type == SlotType.User).AsEnumerable().OrderByDescending(s => s.Thumbsup).FirstOrDefault();
    public override IEnumerable<Slot> GetSlots
        (Database database, int pageStart, int pageSize)
        => database.Slots.ByGameVersion(GameVersion.LittleBigPlanet3, false, true)
        .AsEnumerable()
            .OrderByDescending(s => s.Thumbsup)
            .ThenBy(_ => rand.Next())
            .Skip(Math.Max(0, pageStart - 1))
            .Take(Math.Min(pageSize, 20));
    public override int GetTotalSlots(Database database) => database.Slots.Count(s => s.Type == SlotType.User);
}