namespace LBPUnion.ProjectLighthouse.Types.Users;

public static class PlatformExtensions
{

    public static bool IsPSN(this Platform platform)
    {
        return platform == Platform.PS3 || platform == Platform.PSP || platform == Platform.Vita;
    }
}

public enum Platform
{
    PS3 = 0,
    RPCS3 = 1,
    Vita = 2,
    PSP = 3,
    UnitTest = 4,
    Unknown = -1,
}