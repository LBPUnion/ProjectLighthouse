#nullable enable
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Administration;
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
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

    public static async Task<int> GetPlaceholderSlotId(Database database, int guid, SlotType slotType)
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

            Location? devLocation = await database.Locations.FirstOrDefaultAsync(l => l.Id == 1);
            if (devLocation == null)
            {
                devLocation = new Location
                {
                    Id = 1,
                };
                database.Locations.Add(devLocation);
            }

            int devCreatorId = await database.Users.Where(u => u.Username.Length == 0).Select(u => u.UserId).FirstOrDefaultAsync();
            if (devCreatorId == 0)
            {
                User devCreator = new()
                {
                    Username = "",
                    PermissionLevel = PermissionLevel.Banned,
                    Biography = "Placeholder author of story levels",
                    BannedReason = "Banned to not show in users list",
                    LocationId = devLocation.Id,
                };
                database.Users.Add(devCreator);
                await database.SaveChangesAsync();
                devCreatorId = devCreator.UserId;
            }

            Slot slot = new()
            {
                Name = $"{slotType} slot {guid}",
                Description = $"Placeholder for {slotType} type level",
                CreatorId = devCreatorId,
                InternalSlotId = guid,
                LocationId = devLocation.Id,
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