#nullable enable
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.PlayerData;

public static class LastContactHelper
{
    private static readonly Database database = new();

    public static async Task SetLastContact(User user, GameVersion gameVersion, Platform platform)
    {
        LastContact? lastContact = await database.LastContacts.Where(l => l.UserId == user.UserId).FirstOrDefaultAsync();

        // below makes it not look like trash
        // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
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