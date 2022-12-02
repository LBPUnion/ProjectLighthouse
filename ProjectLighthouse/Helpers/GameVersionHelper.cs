using System.Linq;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.Types;

namespace LBPUnion.ProjectLighthouse.Helpers;

public class GameVersionHelper
{
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

    // https://www.serialstation.com/games/b62d53d9-fdff-4463-8134-64b81e1cbd50
    // includes PS4 games
    public static readonly string[] LittleBigPlanet3TitleIds =
    {
        //PS3
        "BCES02068",
        "BCAS20322",
        "BCJS30095",
        "BCES01663",
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

    public static GameVersion FromTitleId(string titleId)
    {
        if (LittleBigPlanet1TitleIds.Contains(titleId)) return GameVersion.LittleBigPlanet1;
        if (LittleBigPlanet2TitleIds.Contains(titleId)) return GameVersion.LittleBigPlanet2;
        if (LittleBigPlanet3TitleIds.Contains(titleId)) return GameVersion.LittleBigPlanet3;
        if (LittleBigPlanetVitaTitleIds.Contains(titleId)) return GameVersion.LittleBigPlanetVita;
        if (LittleBigPlanetPSPTitleIds.Contains(titleId)) return GameVersion.LittleBigPlanetPSP;

        return GameVersion.Unknown;
    }
}
