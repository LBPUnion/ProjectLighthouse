namespace LBPUnion.ProjectLighthouse.Types;

public enum GameVersion
{
    LittleBigPlanet1 = 0,
    LittleBigPlanet2 = 1,
    LittleBigPlanet3 = 2,
    LittleBigPlanetVita = 3,
    LittleBigPlanetPSP = 4,
    Unknown = -1,
}

public static class GameVersionExtensions
{
    public static string ToPrettyString(this GameVersion gameVersion) => gameVersion.ToString().Replace("LittleBigPlanet", "LittleBigPlanet ");
}