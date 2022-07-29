#nullable enable
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class SlotHelper
{

    public static bool isTypeInvalid(string? slotType)
    {
        if (slotType == null) return true;
        return slotType switch
        {
            "developer" => false,
            "user" => false,
            _ => true,
        };
    }

    public static async Task<int> GetDevSlotId(Database database, int guid)
    {
        int slotId = await database.Slots.Where(s => s.Type == "developer" && s.InternalSlotId == guid).Select(s => s.SlotId).FirstOrDefaultAsync();
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

        User? devCreator = await database.Users.FirstOrDefaultAsync(u => u.Username.Length == 0);
        if (devCreator == null)
        {
            devCreator = new User
            {
                Username = "",
                Banned = true,
                Biography = "Placeholder author of story levels",
                BannedReason = "Banned to not show in users list",
                LocationId = devLocation.Id,
            };
            database.Users.Add(devCreator);
            await database.SaveChangesAsync();
        }

        Console.WriteLine(@"unable to find developer slot with id " + guid);
        Console.WriteLine(@"dev creator id=" + devCreator.UserId);
        Slot slot = new()
        {
            Name = $"Dev slot {guid}",
            Description = "Placeholder for story mode level",
            CreatorId = devCreator.UserId,
            InternalSlotId = guid,
            LocationId = devLocation.Id,
            Type = "developer",
        };
        int doubleCheck = await database.Slots.Where(s => s.Type == "developer" && s.InternalSlotId == guid).Select(s => s.SlotId).FirstOrDefaultAsync();
        if (doubleCheck != 0) return doubleCheck;

        database.Slots.Add(slot);
        await database.SaveChangesAsync();
        return slot.SlotId;

    }
    
}