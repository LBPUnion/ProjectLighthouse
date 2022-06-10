using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class StatisticsHelper
{
    private static readonly Database database = new();

    public static async Task<int> RecentMatches() => await database.LastContacts.Where(l => TimeHelper.Timestamp - l.Timestamp < 300).CountAsync();

    public static async Task<int> RecentMatchesForGame
        (GameVersion gameVersion)
        => await database.LastContacts.Where(l => TimeHelper.Timestamp - l.Timestamp < 300 && l.GameVersion == gameVersion).CountAsync();

    public static async Task<int> SlotCount() => await database.Slots.CountAsync();

    public static async Task<int> UserCount() => await database.Users.CountAsync(u => !u.Banned);

    public static async Task<int> TeamPickCount() => await database.Slots.CountAsync(s => s.TeamPick);

    public static async Task<int> PhotoCount() => await database.Photos.CountAsync();

    public static async Task<int> ReportCount() => await database.Reports.CountAsync();
}