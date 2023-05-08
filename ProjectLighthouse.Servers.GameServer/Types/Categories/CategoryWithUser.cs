#nullable enable
using System.Diagnostics;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Filter;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Serialization;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Categories;

public abstract class CategoryWithUser : Category
{
    
    public abstract IQueryable<SlotEntity> GetSlots(DatabaseContext database, UserEntity user, SlotQueryBuilder queryBuilder);
    public override IQueryable<SlotEntity> GetSlots(DatabaseContext database, SlotQueryBuilder queryBuilder)
    {
        #if DEBUG
        Logger.Error("tried to get slots without user on CategoryWithUser", LogArea.Category);
        if (Debugger.IsAttached) Debugger.Break();
        #endif
        return new List<SlotEntity>().AsQueryable();
    }

    public new static Task<GameCategory> Serialize(DatabaseContext database, SlotQueryBuilder queryBuilder)
    {
        Logger.Error("tried to serialize without user on CategoryWithUser", LogArea.Category);
        return Task.FromResult(new GameCategory());
    }

    public async Task<GameCategory> Serialize(DatabaseContext database, UserEntity user, SlotQueryBuilder queryBuilder)
    {
        List<SlotBase> slots = new();
        SlotEntity? previewSlot = await this.GetSlots(database, user, queryBuilder).FirstOrDefaultAsync();
        if (previewSlot != null)
            slots.Add(SlotBase.CreateFromEntity(previewSlot, GameVersion.LittleBigPlanet3, user.UserId));

        int totalSlots = await this.GetSlots(database, user, queryBuilder).CountAsync();
        return GameCategory.CreateFromEntity(this, new GenericSlotResponse(slots, totalSlots, 2));
    }
}