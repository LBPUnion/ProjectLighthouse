#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Filter;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Serialization;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Types.Levels;

public abstract class Category
{
    public abstract string Name { get; set; }

    public abstract string Description { get; set; }

    public abstract string IconHash { get; set; }

    public abstract string Endpoint { get; set; }

    public string IngameEndpoint => $"/searches/{this.Endpoint}";

    public abstract IQueryable<SlotEntity> GetSlots(DatabaseContext database, SlotQueryBuilder queryBuilder);

    public async Task<GameCategory> Serialize(DatabaseContext database, SlotQueryBuilder queryBuilder)
    {
        List<SlotBase> slots = new();
        SlotEntity? previewSlot = await this.GetSlots(database, queryBuilder).FirstOrDefaultAsync();
        if (previewSlot != null)
            slots.Add(SlotBase.CreateFromEntity(previewSlot, GameVersion.LittleBigPlanet3, -1));

        int totalSlots = await this.GetSlots(database, queryBuilder).CountAsync();
        return GameCategory.CreateFromEntity(this, new GenericSlotResponse(slots, totalSlots, 2));
    }
}