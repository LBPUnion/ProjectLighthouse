using System.Collections.Generic;
using Redis.OM.Modeling;

namespace LBPUnion.ProjectLighthouse.Types.Users;

[Document(StorageType = StorageType.Json, Prefixes = new[] {"UserFriendData",})]
public class UserFriendData
{
    private int userId;
    
    [Indexed]
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