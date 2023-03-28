#nullable enable
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class SlotHelper
{

    public static SlotType ParseType(string? slotType)
    {
        if (slotType == null) return SlotType.Unknown;
        return slotType switch
        {
            "developer" => SlotType.Developer,
            "user" => SlotType.User,
            "moon" => SlotType.Moon,
            "pod" => SlotType.Pod,
            "local" => SlotType.Local,
            _ => SlotType.Unknown,
        };

    }

    public static bool IsTypeInvalid(string? slotType)
    {
        if (slotType == null) return true;
        return slotType switch
        {
            "developer" => false,
            "user" => false,
            _ => true,
        };
    }

    private static readonly SemaphoreSlim semaphore = new(1, 1);

    public static async Task<int> GetPlaceholderUserId(DatabaseContext database)
    {
        int devCreatorId = await database.Users.Where(u => u.Username.Length == 0)
            .Select(u => u.UserId)
            .FirstOrDefaultAsync();
        if (devCreatorId != 0) return devCreatorId;

        await semaphore.WaitAsync(TimeSpan.FromSeconds(5));
        try
        {
            UserEntity devCreator = new()
            {
                Username = "",
                PermissionLevel = PermissionLevel.Banned,
                Biography = "Placeholder author of story levels",
                BannedReason = "Banned to not show in users list",
            };
            database.Users.Add(devCreator);
            await database.SaveChangesAsync();
            return devCreator.UserId;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public static async Task<int> GetPlaceholderSlotId(DatabaseContext database, int guid, SlotType slotType)
    {
        int slotId = await database.Slots.Where(s => s.Type == slotType && s.InternalSlotId == guid).Select(s => s.SlotId).FirstOrDefaultAsync();
        if (slotId != 0) return slotId;

        await semaphore.WaitAsync(TimeSpan.FromSeconds(5));
        try
        {
            // if two requests come in at the same time for the same story level which hasn't been generated
            // one will wait for the lock to be released and the second will be caught by this second check
            slotId = await database.Slots
                           .Where(s => s.Type == slotType && s.InternalSlotId == guid)
                           .Select(s => s.SlotId)
                           .FirstOrDefaultAsync();

            if (slotId != 0) return slotId;

            int devCreatorId = await GetPlaceholderUserId(database);

            SlotEntity slot = new()
            {
                Name = $"{slotType} slot {guid}",
                Description = $"Placeholder for {slotType} type level",
                CreatorId = devCreatorId,
                InternalSlotId = guid,
                Type = slotType,
            };

            database.Slots.Add(slot);
            await database.SaveChangesAsync();
            return slot.SlotId;
        }
        finally
        {
            semaphore.Release();
        }
    }
}