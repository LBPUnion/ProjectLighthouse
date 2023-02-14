using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class StatisticsHelper
{

    public static async Task<int> RecentMatches(Database database) => await database.LastContacts.Where(l => TimeHelper.Timestamp - l.Timestamp < 300).CountAsync();

    public static async Task<int> RecentMatchesForGame(Database database, GameVersion gameVersion)
        => await database.LastContacts.Where(l => TimeHelper.Timestamp - l.Timestamp < 300 && l.GameVersion == gameVersion).CountAsync();

    public static async Task<int> SlotCount(Database database) => await database.Slots.Where(s => s.Type == SlotType.User).CountAsync();

    public static async Task<int> SlotCountForGame(Database database, GameVersion gameVersion, bool includeSublevels = false) => await database.Slots.ByGameVersion(gameVersion, includeSublevels).CountAsync();

    public static async Task<int> UserCount(Database database) => await database.Users.CountAsync(u => u.PermissionLevel != PermissionLevel.Banned);

    public static async Task<int> TeamPickCount(Database database) => await database.Slots.CountAsync(s => s.TeamPick);

    public static async Task<int> PhotoCount(Database database) => await database.Photos.CountAsync();
    
    #region Moderator/Admin specific
    public static async Task<int> ReportCount(Database database) => await database.Reports.CountAsync();
    public static async Task<int> CaseCount(Database database) => await database.Cases.CountAsync();
    public static async Task<int> DismissedCaseCount(Database database) => await database.Cases.CountAsync(c => c.DismissedAt != null);
    #endregion

    public static async Task<int> ApiKeyCount(Database database) => await database.APIKeys.CountAsync();
}