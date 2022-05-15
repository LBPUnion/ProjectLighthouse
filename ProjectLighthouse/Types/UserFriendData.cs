using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Redis.OM.Modeling;

namespace LBPUnion.ProjectLighthouse.Types;

[SuppressMessage("ReSharper", "CollectionNeverQueried.Global")]
[Document(StorageType = StorageType.Json)]
public class UserFriendData
{
    private int userId;
    public int UserId {
        get => this.userId;
        set {
            this.RedisId = value.ToString();
            this.userId = value;
        }
    }

    [RedisIdField]
    public string RedisId { get; set; }

    [Indexed]
    public List<int> FriendIds { get; set; }

    [Indexed]
    public List<int> BlockedIds { get; set; }
}