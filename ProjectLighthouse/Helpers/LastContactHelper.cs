#nullable enable
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class LastContactHelper
{

    public static async Task SetLastContact(Database database, User user, GameVersion gameVersion, Platform platform)
    {
        LastContact? lastContact = await database.LastContacts.Where(l => l.UserId == user.UserId).FirstOrDefaultAsync();

        if (lastContact == null)
        {
            lastContact = new LastContact
            {
                UserId = user.UserId,
            };
            database.LastContacts.Add(lastContact);
        }

        lastContact.Timestamp = TimeHelper.Timestamp;
        lastContact.GameVersion = gameVersion;
        lastContact.Platform = platform;

        await database.SaveChangesAsync();
    }
}