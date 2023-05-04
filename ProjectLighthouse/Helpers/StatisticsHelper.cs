using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Matchmaking.Rooms;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class StatisticsHelper
{

    public static async Task<int> RecentMatches(DatabaseContext database) => await database.LastContacts.Where(l => TimeHelper.Timestamp - l.Timestamp < 300).CountAsync();

    public static async Task<int> RecentMatchesForGame(DatabaseContext database, GameVersion gameVersion)
        => await database.LastContacts.Where(l => TimeHelper.Timestamp - l.Timestamp < 300 && l.GameVersion == gameVersion).CountAsync();

    public static async Task<int> SlotCount(DatabaseContext database) => await database.Slots.Where(s => s.Type == SlotType.User).CountAsync();

    public static async Task<int> SlotCountForGame(DatabaseContext database, GameVersion gameVersion, bool includeSublevels = false) => await database.Slots.ByGameVersion(gameVersion, includeSublevels).CountAsync();

    public static async Task<int> UserCount(DatabaseContext database) => await database.Users.CountAsync(u => u.PermissionLevel != PermissionLevel.Banned);

    public static int RoomCountForPlatform(Platform targetPlatform) => RoomHelper.Rooms.Count(r => r.IsLookingForPlayers && r.RoomPlatform == targetPlatform);

    public static async Task<int> TeamPickCount(DatabaseContext database) => await database.Slots.CountAsync(s => s.TeamPick);

    public static async Task<int> TeamPickCountForGame(DatabaseContext database, GameVersion gameVersion, bool? crosscontrol = null) => await database.Slots.ByGameVersion(gameVersion).CountAsync(s => s.TeamPick && (crosscontrol == null || s.CrossControllerRequired == crosscontrol));

    public static async Task<int> PhotoCount(DatabaseContext database) => await database.Photos.CountAsync();
    
    #region Moderator/Admin specific
    public static async Task<int> ReportCount(DatabaseContext database) => await database.Reports.CountAsync();
    public static async Task<int> CaseCount(DatabaseContext database) => await database.Cases.CountAsync();
    public static async Task<int> DismissedCaseCount(DatabaseContext database) => await database.Cases.CountAsync(c => c.DismissedAt != null);
    #endregion

    public static async Task<int> ApiKeyCount(DatabaseContext database) => await database.APIKeys.CountAsync();
}