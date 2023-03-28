#nullable enable
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class LastContactHelper
{

    public static async Task SetLastContact(DatabaseContext database, UserEntity user, GameVersion gameVersion, Platform platform)
    {
        LastContactEntity? lastContact = await database.LastContacts.Where(l => l.UserId == user.UserId).FirstOrDefaultAsync();

        if (lastContact == null)
        {
            lastContact = new LastContactEntity
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