#nullable enable
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Profiles;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class LastContactHelper
{
    private static readonly Database database = new();

    public static async Task SetLastContact(User user, GameVersion gameVersion)
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

        lastContact.Timestamp = TimestampHelper.Timestamp;
        lastContact.GameVersion = gameVersion;

        await database.SaveChangesAsync();
    }
}