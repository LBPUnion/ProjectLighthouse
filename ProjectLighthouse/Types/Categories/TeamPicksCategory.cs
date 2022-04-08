#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using LBPUnion.ProjectLighthouse.Helpers.Extensions;
using LBPUnion.ProjectLighthouse.Types.Levels;

namespace LBPUnion.ProjectLighthouse.Types.Categories;

public class TeamPicksCategory : Category
{
    public override string Name { get; set; } = "Team Picks";
    public override string Description { get; set; } = "Levels given awards by your instance admin";
    public override string IconHash { get; set; } = "g820626";
    public override string Endpoint { get; set; } = "team_picks";
    public override Slot? GetPreviewSlot(Database database) => database.Slots.OrderByDescending(s => s.FirstUploaded).FirstOrDefault(s => s.TeamPick);
    public override IEnumerable<Slot> GetSlots
        (Database database, int pageStart, int pageSize)
        => database.Slots.ByGameVersion
                (GameVersion.LittleBigPlanet3, false, true)
            .OrderByDescending(s => s.FirstUploaded)
            .Where(s => s.TeamPick)
            .Skip(pageStart - 1)
            .Take(Math.Min(pageSize, 20));
    public override int GetTotalSlots(Database database) => database.Slots.Count(s => s.TeamPick);
}