using System;

namespace LBPUnion.ProjectLighthouse.Types.Users;

public enum PinSet
{
    LittleBigPlanet = 0,
    Vita = 1,
}

public static class PinSetExtensions
{
    public static PinSet ToPinSet(this GameVersion gameVersion) => gameVersion switch
    {
        GameVersion.LittleBigPlanet2 => PinSet.LittleBigPlanet,
        GameVersion.LittleBigPlanet3 => PinSet.LittleBigPlanet,
        GameVersion.LittleBigPlanetVita => PinSet.Vita,
        _ => throw new ArgumentOutOfRangeException(nameof(gameVersion), gameVersion, null),
    };
}
