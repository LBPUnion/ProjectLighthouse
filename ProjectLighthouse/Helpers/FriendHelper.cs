using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace LBPUnion.ProjectLighthouse.Helpers;

[NotMapped]
[SuppressMessage("ReSharper", "CollectionNeverQueried.Global")]
public static class FriendHelper
{
    public static readonly Dictionary<int, int[]> FriendIdsByUserId = new();
    public static readonly Dictionary<int, int[]> BlockedIdsByUserId = new();
}