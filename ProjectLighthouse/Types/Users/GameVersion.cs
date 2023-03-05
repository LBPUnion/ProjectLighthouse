using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Users;

public enum GameVersion
{
    [XmlEnum("0")]
    LittleBigPlanet1 = 0,
    [XmlEnum("1")]
    LittleBigPlanet2 = 1,
    [XmlEnum("2")]
    LittleBigPlanet3 = 2,
    [XmlEnum("3")]
    LittleBigPlanetVita = 3,
    [XmlEnum("4")]
    LittleBigPlanetPSP = 4,
    Unknown = -1,
}

public static class GameVersionExtensions
{
    public static string ToPrettyString(this GameVersion gameVersion) => gameVersion.ToString().Replace("LittleBigPlanet", "LittleBigPlanet ");
}

public static class GameVersionHelper
{
    #region Title IDs
    #region LBP1 Title IDs
    // https://www.serialstation.com/games/b89b4eb4-4e4c-4e54-b72b-f7f9dbfac125
    public static readonly string[] LittleBigPlanet1TitleIds =
    {
        "BCES00141",
        "BCAS20091",
        "BCUS98208",
        "BCAS20078",
        "BCJS70009",
        "BCES00611",
        "BCUS98148",
        "BCAS20058",
        "BCJS30018",
        "BCUS98199",
        "BCJB95003",
        "NPEA00241",
        "NPUA98208",
        "NPHA80092",
        "BCKS10059",
        "BCKS10088",
        "BCUS70030",
        "NPJA00052",
        "NPUA80472",
        //Debug, Beta and Demo
        "BCET70011",
        "NPUA70045",
        "NPEA00147",
        "BCET70002",
        "NPHA80067",
        "NPJA90074",
        //Move
        "NPEA00243",
        "NPUA80479",
        "NPHA80093",
        "NPJA00058",
    };
    #endregion

    #region LBP2 Title IDs
    // https://serialstation.com/games/35e69aba-1872-4fd7-9d39-11ce75924040
    public static readonly string[] LittleBigPlanet2TitleIds =
    {
        "BCUS98249",
        "BCES01086",
        "BCAS20113",
        "BCJS70024",
        "BCAS20201",
        "BCUS98245",
        "BCES01345",
        "BCJS30058",
        "BCUS98372",
        "BCES00850",
        "BCES01346",
        "BCUS90260",
        "BCES01694",
        "NPUA80662",
        "NPEA00324",
        "NPEA00437",
        "BCES01693",
        "BCKS10150",
        //Debug, Beta and Demo
        "NPUA70117",
        "BCET70023",
        "BCET70035",
        "NPEA90077",
        "NPEA90098",
        "NPHA80113",
        "NPHA80125",
        "NPJA90152",
        "NPUA70127",
        "NPUA70169",
        "NPUA70174",
        //HUB
        "BCET70055",
        "NPEA00449",
        "NPHA80261",
        "NPUA80967",
    };
    #endregion

    #region LBP3 Title IDs
    // https://www.serialstation.com/games/b62d53d9-fdff-4463-8134-64b81e1cbd50
    // includes PS4 games
    public static readonly string[] LittleBigPlanet3TitleIds =
    {
        //PS3
        "BCES02068",
        "BCAS20322",
        "BCJS30095",
        "BCUS98362",
        "NPUA81116",
        "NPEA00515",
        "BCUS81138",
        "NPJA00123",
        "NPHO00189",
        "NPHA80277",
        //Debug, Beta and Demo
        "NPEA90128",
        "NPUA81174",
        "BCES01663",
        //PS4
        "CUSA00693",
        "CUSA00810",
        "CUSA00738",
        "PCJS50003",
        "CUSA00063",
        "PCKS90007",
        "PCAS00012",
        "CUSA00601",
        "CUSA00762",
        "PCAS20007",
        "CUSA00473",
        //Debug, Beta and Demo
        "CUSA01072",
        "CUSA01077",
        "CUSA01304",
    };
    #endregion

    #region LBPVita Title IDs
    public static readonly string[] LittleBigPlanetVitaTitleIds =
    {
        "PCSF00021",
        "PCSA00017",
        "PCSC00013",
        "PCSD00006",
        "PCSA00549",
        "PCSF00516",
        "PCSA22018",
        "PCSA22106",
        "PCSD00039",
        "VCAS32010",
        "VCJS10006",
        "VCKS62003",
        //Debug, Beta and Demo
        "PCSA00061",
        "PCSA00078",
        "PCSA00081",
        "PCSF00152",
        "PCSF00188",
        "PCSF00211",
    };
    #endregion

    #region LBPPSP Title IDs
    public static readonly string[] LittleBigPlanetPSPTitleIds =
    {
        "NPWR00500",
        "UCAS40262",
        "UCES01264",
        "UCUS98744",
        "UCJS10107",
        "NPHG00033",
        "NPJG00073",
        "NPJG90072",
        //Debug, Beta and Demo
        "NPHG00035",
        "NPUG70064",
        "NPEG90019",
    };
    #endregion
    #endregion

    private static readonly Dictionary<string, GameVersion> titleIdMap = createTitleMap();

    private static Dictionary<string, GameVersion> createTitleMap()
    {
        Dictionary<string, GameVersion> titleMap = new();
        LittleBigPlanet1TitleIds.ToList().ForEach(x => titleMap.Add(x, GameVersion.LittleBigPlanet1));
        LittleBigPlanet2TitleIds.ToList().ForEach(x => titleMap.Add(x, GameVersion.LittleBigPlanet2));
        LittleBigPlanet3TitleIds.ToList().ForEach(x => titleMap.Add(x, GameVersion.LittleBigPlanet3));
        LittleBigPlanetVitaTitleIds.ToList().ForEach(x => titleMap.Add(x, GameVersion.LittleBigPlanetVita));
        LittleBigPlanetPSPTitleIds.ToList().ForEach(x => titleMap.Add(x, GameVersion.LittleBigPlanetPSP));

        return titleMap;
    }

    public static GameVersion FromTitleId(string titleId) 
        => titleIdMap.TryGetValue(titleId, out GameVersion parsedVersion) ? parsedVersion : GameVersion.Unknown;
}