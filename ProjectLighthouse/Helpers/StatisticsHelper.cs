using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Administration;
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class StatisticsHelper
{
    private static readonly Database database = new();

    public static async Task<int> RecentMatches() => await database.LastContacts.Where(l => TimeHelper.Timestamp - l.Timestamp < 300).CountAsync();

    public static async Task<int> RecentMatchesForGame(GameVersion gameVersion)
        => await database.LastContacts.Where(l => TimeHelper.Timestamp - l.Timestamp < 300 && l.GameVersion == gameVersion).CountAsync();

    public static async Task<int> SlotCount() => await database.Slots.Where(s => s.Type == SlotType.User).CountAsync();

    public static async Task<int> UserCount() => await database.Users.CountAsync(u => u.PermissionLevel != PermissionLevel.Banned);

    public static async Task<int> TeamPickCount() => await database.Slots.CountAsync(s => s.TeamPick);

    public static async Task<int> PhotoCount() => await database.Photos.CountAsync();
    
    #region Moderator/Admin specific
    public static async Task<int> ReportCount() => await database.Reports.CountAsync();
    public static async Task<int> CaseCount() => await database.Cases.CountAsync();
    public static async Task<int> DismissedCaseCount() => await database.Cases.CountAsync(c => c.DismissedAt != null);
    #endregion

    public static async Task<int> APIKeyCount() => await database.APIKeys.CountAsync();
}