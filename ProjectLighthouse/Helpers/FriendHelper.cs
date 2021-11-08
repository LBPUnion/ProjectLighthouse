using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace LBPUnion.ProjectLighthouse.Helpers
{
    [NotMapped]
    public static class FriendHelper
    {
        public static Dictionary<int, int[]> FriendIdsByUserId = new();
        public static Dictionary<int, int[]> BlockedIdsByUserId = new();
    }
}