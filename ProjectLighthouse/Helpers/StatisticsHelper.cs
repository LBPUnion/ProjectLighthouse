using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class StatisticsHelper
{
    private static readonly Database database = new();

    public static async Task<int> RecentMatches() => await database.LastContacts.Where(l => TimestampHelper.Timestamp - l.Timestamp < 300).CountAsync();

    public static async Task<int> SlotCount() => await database.Slots.CountAsync();

    public static async Task<int> UserCount() => await database.Users.CountAsync(u => !u.Banned);

    public static async Task<int> MMPicksCount() => await database.Slots.CountAsync(s => s.TeamPick);

    public static async Task<int> PhotoCount() => await database.Photos.CountAsync();
}