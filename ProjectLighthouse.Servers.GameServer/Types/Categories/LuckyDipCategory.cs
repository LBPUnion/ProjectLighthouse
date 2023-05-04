#nullable enable
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Categories;

public class LuckyDipCategory : Category
{
    public override string Name { get; set; } = "Lucky Dip";
    public override string Description { get; set; } = "A random selection of content";
    public override string IconHash { get; set; } = "g820605";
    public override string Endpoint { get; set; } = "lbp2luckydip";
    public override SlotEntity? GetPreviewSlot(DatabaseContext database) => database.Slots.Where(s => s.Type == SlotType.User && !s.CrossControllerRequired).OrderByDescending(_ => EF.Functions.Random()).FirstOrDefault();
    public override IQueryable<SlotEntity> GetSlots
        (DatabaseContext database, int pageStart, int pageSize)
        => database.Slots.ByGameVersion(GameVersion.LittleBigPlanet3, false, true)
            .Where(s => !s.CrossControllerRequired)
            .OrderByDescending(_ => EF.Functions.Random())
            .Skip(Math.Max(0, pageStart - 1))
            .Take(Math.Min(pageSize, 20));
    public override int GetTotalSlots(DatabaseContext database) => database.Slots.Count(s => s.Type == SlotType.User);
}