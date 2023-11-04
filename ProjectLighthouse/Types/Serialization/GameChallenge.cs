using System.Collections.Generic;
using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

[XmlRoot("Challenge_header")]
public class GameChallenge : ILbpSerializable
{
    [XmlElement("Total_challenges")]
    public int TotalChallenges { get; set; }

    [XmlElement("Challenge_End_Date")]
    public ulong EndDate { get; set; }

    [XmlElement("Challenge_Top_Rank_Bronze_Range")]
    public float BronzeRankPercentage { get; set; }

    [XmlElement("Challenge_Top_Rank_Silver_Range")]
    public float SilverRankPercentage { get; set; }

    [XmlElement("Challenge_Top_Rank_Gold_Range")]
    public float GoldRankPercentage { get; set; }

    [XmlElement("Challenge_CycleTime")]
    public ulong CycleTime { get; set; }

    // ReSharper disable once IdentifierTypo
    [XmlElement("item_data")]
    public List<GameChallenge> Challenges { get; set; }

    #region Server challenge configuration

    public static GameChallenge ServerChallenges() => new()
    {
        TotalChallenges = 14,
        EndDate = 1494460860000000,
        TopRankBronzePercentage = 0.51m,
        TopRankSilverPercentage = 0.26m,
        TopRankGoldPercentage = 0.11m,
        CycleTime = 1209600000000,
        ChallengeItemDatas = new List<ChallengeItemData>
        {
            new()
            {
                Id = 0,
                ActiveDateStarts = 1476096166000000,
                ActiveDateEnds = 1478736060000000,
                LamsDescriptionId = "CHALLENGE_NEWTONBOUNCE_DESC",
                LamsTitleId = "CHALLENGE_NEWTONBOUNCE_NAME",
                PinId = 3003874881,
                RankPin = 2922567456,
                ContentName = "TG_LittleBigPlanet3",
                PlanetUser = "qd3c781a5a6-GBen",
                PlanetId = 1085260,
                Photo1 = 1112639,
            },
            new()
            {
                Id = 1,
                ActiveDateStarts = 1478736060000000,
                ActiveDateEnds = 1479945660000000,
                LamsDescriptionId = "CHALLENGE_SCREENCHASE_DESC",
                LamsTitleId = "CHALLENGE_SCREENCHASE_NAME",
                PinId = 282407472,
                RankPin = 3340696069,
                ContentName = "TG_LittleBigPlanet2",
                PlanetUser = "qd3c781a5a6-GBen",
                PlanetId = 1102387,
                Photo1 = 1112651,
            },
            new()
            {
                Id = 2,
                ActiveDateStarts = 1479945660000000,
                ActiveDateEnds = 1481155260000000,
                LamsDescriptionId = "CHALLENGE_RABBITBOXING_DESC",
                LamsTitleId = "CHALLENGE_RABBITBOXING_NAME",
                PinId = 2529088759,
                RankPin = 958144818,
                ContentName = "TG_LittleBigPlanet2",
                PlanetUser = "qd3c781a5a6-GBen",
                PlanetId = 1085264,
                Photo1 = 1112627,
            },
            new()
            {
                Id = 3,
                ActiveDateStarts = 1481155260000000,
                ActiveDateEnds = 1481414460000000,
                LamsDescriptionId = "CHALLENGE_FLOATYFLUID_DESC",
                LamsTitleId = "CHALLENGE_FLOATYFLUID_NAME",
                PinId = 183892581,
                RankPin = 3442917932,
                Content = "LBPDLCNISBLK0001",
                ContentName = "SBSP_THEME_PACK_NAME",
                PlanetUser = "qd3c781a5a6-GBen",
                PlanetId = 1095449,
                Photo1 = 1112619,
            },
            new()
            {
                Id = 4,
                ActiveDateStarts = 1481414460000000,
                ActiveDateEnds = 1483574460000000,
                LamsDescriptionId = "CHALLENGE_ISLANDRACE_DESC",
                LamsTitleId = "CHALLENGE_ISLANDRACE_NAME",
                PinId = 315245769,
                RankPin = 443310584,
                ContentName = "TG_LittleBigPlanet",
                PlanetUser = "qd3c781a5a6-GBen",
                PlanetId = 1102858,
                Photo1 = 1112655,
            },
            new()
            {
                Id = 5,
                ActiveDateStarts = 1483574460000000,
                ActiveDateEnds = 1484784060000000,
                LamsDescriptionId = "CHALLENGE_SPACEDODGING_DESC",
                LamsTitleId = "CHALLENGE_SPACEDODGING_NAME",
                PinId = 144212050,
                RankPin = 2123417147,
                ContentName = "TG_LittleBigPlanet3",
                PlanetUser = "qd3c781a5a6-GBen",
                PlanetId = 1085266,
                Photo1 = 1112667,
            },
            new()
            {
                Id = 6,
                ActiveDateStarts = 1484784060000000,
                ActiveDateEnds = 1485993660000000,
                LamsDescriptionId = "CHALLENGE_INVISIBLECIRCUIT_DESC",
                LamsTitleId = "CHALLENGE_INVISIBLECIRCUIT_NAME",
                PinId = 249569175,
                RankPin = 1943114258,
                ContentName = "TG_LittleBigPlanet",
                PlanetUser = "qd3c781a5a6-GBen",
                PlanetId = 1096814,
                Photo1 = 1112635,
            },
            new()
            {
                Id = 7,
                ActiveDateStarts = 1485993660000000,
                ActiveDateEnds = 1487203260000000,
                LamsDescriptionId = "CHALLENGE_HOVERBOARDRAILS_DESC",
                LamsTitleId = "CHALLENGE_HOVERBOARDRAILS_NAME",
                PinId = 3478661003,
                RankPin = 592022798,
                Content = "LBPDLCBTTFLK0001",
                ContentName = "BTTF_LEVEL_KIT_NAME",
                PlanetUser = "qd3c781a5a6-GBen",
                PlanetId = 1085256,
                Photo1 = 1112623,
            },
            new()
            {
                Id = 8,
                ActiveDateStarts = 1487203260000000,
                ActiveDateEnds = 1488412860000000,
                LamsDescriptionId = "CHALLENGE_TOWERBOOST_DESC",
                LamsTitleId = "CHALLENGE_TOWERBOOST_NAME",
                PinId = 216730878,
                RankPin = 545532447,
                ContentName = "TG_LittleBigPlanet2",
                PlanetUser = "qd3c781a5a6-GBen",
                PlanetId = 1092504,
                Photo1 = 1112671,
            },
            new()
            {
                Id = 9,
                ActiveDateStarts = 1488412860000000,
                ActiveDateEnds = 1489622460000000,
                LamsDescriptionId = "CHALLENGE_SWOOPPANELS_DESC",
                LamsTitleId = "CHALLENGE_SWOOPPANELS_NAME",
                PinId = 2054302637,
                RankPin = 3288689476,
                ContentName = "TG_LittleBigPlanet2",
                PlanetUser = "qd3c781a5a6-GBen",
                PlanetId = 1085268,
                Photo1 = 1112643,
            },
            new()
            {
                Id = 10,
                ActiveDateStarts = 1489622460000000,
                ActiveDateEnds = 1490832060000000,
                LamsDescriptionId = "CHALLENGE_PINBALLCRYPTS_DESC",
                LamsTitleId = "CHALLENGE_PINBALLCRYPTS_NAME",
                PinId = 618998172,
                RankPin = 4087839785,
                ContentName = "TG_LittleBigPlanet3",
                PlanetUser = "qd3c781a5a6-GBen",
                PlanetId = 1085262,
                Photo1 = 1112647,
            },
            new()
            {
                Id = 11,
                ActiveDateStarts = 1490832060000000,
                ActiveDateEnds = 1492041660000000,
                LamsDescriptionId = "CHALLENGE_TIEHOP_DESC",
                LamsTitleId = "CHALLENGE_TIEHOP_NAME",
                PinId = 3953447125,
                RankPin = 2556445436,
                ContentName = "TG_LittleBigPlanet",
                PlanetUser = "qd3c781a5a6-GBen",
                PlanetId = 1092367,
                Photo1 = 1112659,
            },
            new()
            {
                Id = 12,
                ActiveDateStarts = 1492041660000000,
                ActiveDateEnds = 1493251260000000,
                LamsDescriptionId = "CHALLENGE_JOKERFUNHOUSE_DESC",
                LamsTitleId = "CHALLENGE_JOKERFUNHOUSE_NAME",
                PinId = 1093784294,
                RankPin = 1757295127,
                Content = "LBPDLCWBDCLK0001",
                ContentName = "DCCOMICS_THEME_NAME",
                PlanetUser = "qd3c781a5a6-GBen",
                PlanetId = 1085258,
                Photo1 = 1112631,
            },
            new()
            {
                Id = 13,
                ActiveDateStarts = 1493251260000000,
                ActiveDateEnds = 1494460860000000,
                LamsDescriptionId = "CHALLENGE_DINERSHOOTING_DESC",
                LamsTitleId = "CHALLENGE_DINERSHOOTING_NAME",
                PinId = 1568570416,
                RankPin = 3721717765,
                ContentName = "TG_LittleBigPlanet3",
                PlanetUser = "qd3c781a5a6-GBen",
                PlanetId = 1085254,
                Photo1 = 1112663,
            },
        },
    };

    #endregion
}

public class ChallengeItemData
{
    [XmlAttribute("Challenge_ID")]
    public int Id { get; set; }

    [XmlAttribute("Challenge_active_date_starts")]
    public ulong StartTime { get; set; }

    [XmlAttribute("Challenge_active_date_ends")]
    public ulong ActiveDateEnds { get; set; }

    [XmlAttribute("Challenge_LAMSDescription_Id")]
    public string LamsDescriptionId { get; set; }

    [XmlAttribute("Challenge_LAMSTitle_Id")]
    public string LamsTitleId { get; set; }

    [XmlAttribute("Challenge_PinId")]
    public ulong PinId { get; set; }

    [XmlAttribute("Challenge_RankPin")]
    public ulong RankPin { get; set; }

    [XmlAttribute("Challenge_Content")]
    public string Content { get; set; }

    [XmlAttribute("Challenge_Content_name")]
    public string ContentName { get; set; }

    [XmlAttribute("Challenge_Planet_User")]
    public string PlanetUser { get; set; }

    [XmlAttribute("Challenge_planetId")]
    public ulong PlanetId { get; set; }

    [XmlAttribute("Challenge_photo_1")]
    public ulong Photo1 { get; set; }
}