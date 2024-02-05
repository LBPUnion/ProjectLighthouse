using System.Collections.Concurrent;
using System.Reflection;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using Xunit;

namespace LBPUnion.ProjectLighthouse.Tests.Unit;

[Trait("Category", "Unit")]
public class EmailCooldownTests
{
    private static ConcurrentDictionary<int, long>? GetInternalDict =>
        typeof(SMTPHelper).GetField("recentlySentMail", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null) as ConcurrentDictionary<int, long>;

    /*
     * TODO This way of testing sucks because it relies on internal implementation,
     * but half of this codebase is static singletons so my hand has kinda been forced
     */

    [Fact]
    public void CanSendMail_WhenExpirationReached()
    {
        MethodInfo? canSendMethod = typeof(SMTPHelper).GetMethod("CanSendMail", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(canSendMethod);

        UserEntity userEntity = new()
        {
            UserId = 1,
        };

        Assert.NotNull(GetInternalDict);

        GetInternalDict.Clear();

        GetInternalDict.TryAdd(1, 0);

        bool? canSend = (bool?)canSendMethod.Invoke(null, new object?[] { userEntity, });

        const bool expectedValue = true;

        Assert.NotNull(canSend);
        Assert.Equal(expectedValue, canSend);
    }

    [Fact]
    public void CanSendMail_WhenExpirationNotReached()
    {
        MethodInfo? canSendMethod =
            typeof(SMTPHelper).GetMethod("CanSendMail", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(canSendMethod);

        UserEntity userEntity = new()
        {
            UserId = 1,
        };

        Assert.NotNull(GetInternalDict);

        GetInternalDict.Clear();

        GetInternalDict.TryAdd(1, long.MaxValue);

        bool? canSend = (bool?)canSendMethod.Invoke(null, new object?[] { userEntity, });

        const bool expectedValue = false;

        Assert.NotNull(canSend);
        Assert.Equal(expectedValue, canSend);
    }

    [Fact]
    public void CanSendMail_ExpiredEntriesAreRemoved()
    {
        MethodInfo? canSendMethod = typeof(SMTPHelper).GetMethod("CanSendMail", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(canSendMethod);

        UserEntity userEntity = new()
        {
            UserId = 1,
        };

        Assert.NotNull(GetInternalDict);

        GetInternalDict.Clear();

        GetInternalDict.TryAdd(2, 0);
        GetInternalDict.TryAdd(3, TimeHelper.TimestampMillis - 100);
        GetInternalDict.TryAdd(4, long.MaxValue);

        canSendMethod.Invoke(null, new object?[] { userEntity, });

        Assert.False(GetInternalDict.TryGetValue(2, out _));
        Assert.False(GetInternalDict.TryGetValue(3, out _));
        Assert.True(GetInternalDict.TryGetValue(4, out _));
    }
    
}