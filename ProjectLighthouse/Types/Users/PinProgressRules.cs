using System;

namespace LBPUnion.ProjectLighthouse.Types.Users;

public static class PinProgressRules
{
    private const uint StoryScoreboardBestPercentage = 191183438u;
    private const uint CommunityScoreboardBestPercentage = 2033315234u;

    public static bool IsLowerProgressBetter(uint progressType) => progressType is StoryScoreboardBestPercentage or CommunityScoreboardBestPercentage;

    public static double Merge(uint progressType, double storedValue, double uploadedValue)
    {
        return IsLowerProgressBetter(progressType)
            ? Math.Min(storedValue, uploadedValue)
            : Math.Max(storedValue, uploadedValue);
    }
}
