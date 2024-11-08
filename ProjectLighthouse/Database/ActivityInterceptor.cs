using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Types.Activity;
using LBPUnion.ProjectLighthouse.Types.Entities.Activity;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace LBPUnion.ProjectLighthouse.Database;

public class ActivityInterceptor : SaveChangesInterceptor
{
    private class CustomTrackedEntity
    {
        public required EntityState State { get; init; }
        public required object Entity { get; init; }
        public required object OldEntity { get; init; }
    }

    private struct TrackedEntityKey
    {
        public Type Type { get; set; }
        public int HashCode { get; set; }
        public Guid ContextId { get; set; }
    }

    private readonly ConcurrentDictionary<TrackedEntityKey, CustomTrackedEntity> unsavedEntities;
    private readonly IEntityEventHandler eventHandler;

    public ActivityInterceptor(IEntityEventHandler eventHandler)
    {
        this.eventHandler = eventHandler;
        this.unsavedEntities = new ConcurrentDictionary<TrackedEntityKey, CustomTrackedEntity>();
    }

    #region Hooking stuff

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        this.SaveNewEntities(eventData);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync
        (DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = new())
    {
        this.SaveNewEntities(eventData);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        this.ParseInsertedEntities(eventData);
        return base.SavedChanges(eventData, result);
    }

    public override ValueTask<int> SavedChangesAsync
        (SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = new())
    {
        this.ParseInsertedEntities(eventData);
        return base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    #endregion

    private void SaveNewEntities(DbContextEventData eventData)
    {
        if (eventData.Context == null) return;

        DbContext context = eventData.Context;

        this.unsavedEntities.Clear();

        foreach (EntityEntry entry in context.ChangeTracker.Entries())
        {
            // Ignore activities
            if (entry.Metadata.BaseType?.ClrType == typeof(ActivityEntity) || entry.Metadata.ClrType == typeof(LastContactEntity)) continue;

            // Ignore tokens
            if (entry.Metadata.Name.Contains("Token")) continue;

            if (entry.State is not (EntityState.Added or EntityState.Deleted or EntityState.Modified)) continue;
            this.unsavedEntities.TryAdd(new TrackedEntityKey
                {
                    ContextId = context.ContextId.InstanceId,
                    Type = entry.Entity.GetType(),
                    HashCode = entry.Entity.GetHashCode(),
                },
                new CustomTrackedEntity
                {
                    State = entry.State,
                    Entity = entry.Entity,
                    OldEntity = entry.OriginalValues.ToObject(),
                });
        }
    }

    private void ParseInsertedEntities(DbContextEventData eventData)
    {
        if (eventData.Context is not DatabaseContext context) return;

        HashSet<CustomTrackedEntity> entities = [];

        List<EntityEntry> entries = context.ChangeTracker.Entries().ToList();

        foreach (KeyValuePair<TrackedEntityKey, CustomTrackedEntity> kvp in this.unsavedEntities)
        {
            EntityEntry entry = entries.FirstOrDefault(e =>
                e.Metadata.ClrType == kvp.Key.Type && e.Entity.GetHashCode() == kvp.Key.HashCode);
            switch (kvp.Value.State)
            {
                case EntityState.Added:
                case EntityState.Modified:
                    if (entry != null) entities.Add(kvp.Value);
                    break;
                case EntityState.Deleted:
                    if (entry == null) entities.Add(kvp.Value);
                    break;
                case EntityState.Detached:
                case EntityState.Unchanged:
                default:
                    break;
            }
        }

        foreach (CustomTrackedEntity entity in entities)
        {
            switch (entity.State)
            {
                case EntityState.Added:
                    this.eventHandler.OnEntityInserted(context, entity.Entity);
                    break;
                case EntityState.Deleted:
                    this.eventHandler.OnEntityDeleted(context, entity.Entity);
                    break;
                case EntityState.Modified:
                    this.eventHandler.OnEntityChanged(context, entity.OldEntity, entity.Entity);
                    break;
                case EntityState.Detached:
                case EntityState.Unchanged:
                default:
                    continue;
            }
        }
    }
}