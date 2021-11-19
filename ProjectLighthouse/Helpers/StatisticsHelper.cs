using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Helpers
{
    public static class StatisticsHelper
    {
        private static readonly Database database = new();

        public static async Task<int> RecentMatches() => await database.LastMatches.Where(l => TimestampHelper.Timestamp - l.Timestamp < 120).CountAsync();

        public static async Task<int> SlotCount() => await database.Slots.CountAsync();

        public static async Task<int> MMPicksCount() => await database.Slots.CountAsync(s => s.TeamPick);
    }
}