#nullable enable
using System.Diagnostics;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Serialization;
using LBPUnion.ProjectLighthouse.Types.Users;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Categories;

public abstract class CategoryWithUser : Category
{
    public abstract SlotEntity? GetPreviewSlot(DatabaseContext database, UserEntity user);
    public override SlotEntity? GetPreviewSlot(DatabaseContext database)
    {
        #if DEBUG
        Logger.Error("tried to get preview slot without user on CategoryWithUser", LogArea.Category);
        if (Debugger.IsAttached) Debugger.Break();
        #endif
        return null;
    }

    public abstract int GetTotalSlots(DatabaseContext database, UserEntity user);
    public override int GetTotalSlots(DatabaseContext database)
    {
        #if DEBUG
        Logger.Error("tried to get total slots without user on CategoryWithUser", LogArea.Category);
        if (Debugger.IsAttached) Debugger.Break();
        #endif
        return -1;
    }

    public abstract IQueryable<SlotEntity> GetSlots(DatabaseContext database, UserEntity user, int pageStart, int pageSize);
    public override IList<SlotEntity> GetSlots(DatabaseContext database, int pageStart, int pageSize)
    {
        #if DEBUG
        Logger.Error("tried to get slots without user on CategoryWithUser", LogArea.Category);
        if (Debugger.IsAttached) Debugger.Break();
        #endif
        return new List<SlotEntity>();
    }

    public new string Serialize(DatabaseContext database)
    {
        Logger.Error("tried to serialize without user on CategoryWithUser", LogArea.Category);
        return string.Empty;
    }

    public GameCategory Serialize(DatabaseContext database, UserEntity user)
    {
        List<SlotBase> slots = new();
        SlotEntity? previewSlot = this.GetPreviewSlot(database, user);
        if (previewSlot != null)
            slots.Add(SlotBase.CreateFromEntity(previewSlot, GameVersion.LittleBigPlanet3, user.UserId));
        
        int totalSlots = this.GetTotalSlots(database, user);
        return GameCategory.CreateFromEntity(this, new GenericSlotResponse(slots, totalSlots, 2));
    }
}