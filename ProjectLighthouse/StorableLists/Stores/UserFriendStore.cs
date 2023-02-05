#nullable enable
using System.Collections.Generic;
using System.Linq;
using LBPUnion.ProjectLighthouse.Types.Users;

namespace LBPUnion.ProjectLighthouse.StorableLists.Stores;

public static class UserFriendStore
{
    private static List<UserFriendData>? friendDataStore;
    
    private static StorableList<UserFriendData> getStorableFriendData()
    {
        if (RedisDatabase.Initialized)
        {
            return new RedisStorableList<UserFriendData>(RedisDatabase.UserFriendStoreCollection);
        }

        friendDataStore ??= new List<UserFriendData>();
        return new NormalStorableList<UserFriendData>(friendDataStore);
    }

    public static UserFriendData? GetUserFriendData(int userId) => getStorableFriendData().FirstOrDefault(s => s.UserId == userId);

    public static UserFriendData CreateUserFriendData(int userId)
    {
        UserFriendData friendData = new()
        {
            UserId = userId,
        };

        getStorableFriendData().Add(friendData);
        return friendData;
    }

    public static void UpdateFriendData(UserFriendData friendData)
    {
        getStorableFriendData().Update(friendData);
    }
}