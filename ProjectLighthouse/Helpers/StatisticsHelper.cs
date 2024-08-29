using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Filter;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class StatisticsHelper
{
    public static async Task<int> RecentMatches(DatabaseContext database) => await database.LastContacts.Where(l => TimeHelper.Timestamp - l.Timestamp < 300).CountAsync();

    public static async Task<int> RecentMatches(DatabaseContext database, Expression<Func<LastContactEntity, bool>> contactFilter) =>
        await database.LastContacts.Where(l => TimeHelper.Timestamp - l.Timestamp < 300).Where(contactFilter).CountAsync();

    public static async Task<int> SlotCount(DatabaseContext database, SlotQueryBuilder queryBuilder) => await database.Slots.Where(queryBuilder.Build()).CountAsync();

    public static async Task<int> UserCount(DatabaseContext database) => await database.Users.CountAsync(u => u.PermissionLevel != PermissionLevel.Banned);

    public static int RoomCountForPlatform(Platform targetPlatform) => RoomHelper.Rooms.Count(r => r.IsLookingForPlayers && r.RoomPlatform == targetPlatform);

    public static async Task<int> PhotoCount(DatabaseContext database) => await database.Photos.CountAsync();
    
    #region Moderator/Admin specific
    public static async Task<int> ReportCount(DatabaseContext database) => await database.Reports.CountAsync();
    public static async Task<int> CaseCount(DatabaseContext database) => await database.Cases.CountAsync();
    public static async Task<int> DismissedCaseCount(DatabaseContext database) => await database.Cases.CountAsync(c => c.DismissedAt != null);
    #endregion

    public static async Task<int> ApiKeyCount(DatabaseContext database) => await database.APIKeys.CountAsync();
}