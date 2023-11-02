using System.Collections.Generic;
using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

[XmlRoot("Challenge_header")]
public class GameChallenge : ILbpSerializable
{
    [XmlElement("Total_challenges")]
    public int TotalChallenges { get; set; }

    [XmlElement("Challenge_End_Date")]
    public ulong ChallengeEndDate { get; set; }

    [XmlElement("Challenge_Top_Rank_Bronze_Range")]
    public decimal ChallengeTopRankBronzeRange { get; set; }

    [XmlElement("Challenge_Top_Rank_Silver_Range")]
    public decimal ChallengeTopRankSilverRange { get; set; }

    [XmlElement("Challenge_Top_Rank_Gold_Range")]
    public decimal ChallengeTopRankGoldRange { get; set; }

    [XmlElement("Challenge_CycleTime")]
    public ulong ChallengeCycleTime { get; set; }

    // ReSharper disable once IdentifierTypo
    [XmlElement("Challenge_Item_Datas")]
    public List<ChallengeItemData> ChallengeItemDatas { get; set; }

    #region Official server challenge configuration

    public static GameChallenge OfficialServerChallenge() => new()
    {
        TotalChallenges = 14,
        ChallengeEndDate = 1494460860000000,
        ChallengeTopRankBronzeRange = 0.51m,
        ChallengeTopRankSilverRange = 0.26m,
        ChallengeTopRankGoldRange = 0.11m,
        ChallengeCycleTime = 1209600000000,
        ChallengeItemDatas = new List<ChallengeItemData>
        {
            new()
            {
                ChallengeId = 0,
                ChallengeActiveDateStarts = 1476096166000000,
                ChallengeActiveDateEnds = 1478736060000000,
                ChallengeLamsDescriptionId = "CHALLENGE_NEWTONBOUNCE_DESC",
                ChallengeLamsTitleId = "CHALLENGE_NEWTONBOUNCE_NAME",
                ChallengePinId = 3003874881,
                ChallengeRankPin = 2922567456,
                ChallengeContentName = "TG_LittleBigPlanet3",
                ChallengePlanetUser = "qd3c781a5a6-GBen",
                ChallengePlanetId = 1085260,
                ChallengePhoto1 = 1112639,
            },
            new()
            {
                ChallengeId = 1,
                ChallengeActiveDateStarts = 1478736060000000,
                ChallengeActiveDateEnds = 1479945660000000,
                ChallengeLamsDescriptionId = "CHALLENGE_SCREENCHASE_DESC",
                ChallengeLamsTitleId = "CHALLENGE_SCREENCHASE_NAME",
                ChallengePinId = 282407472,
                ChallengeRankPin = 3340696069,
                ChallengeContentName = "TG_LittleBigPlanet2",
                ChallengePlanetUser = "qd3c781a5a6-GBen",
                ChallengePlanetId = 1102387,
                ChallengePhoto1 = 1112651,
            },
            new()
            {
                ChallengeId = 2,
                ChallengeActiveDateStarts = 1479945660000000,
                ChallengeActiveDateEnds = 1481155260000000,
                ChallengeLamsDescriptionId = "CHALLENGE_RABBITBOXING_DESC",
                ChallengeLamsTitleId = "CHALLENGE_RABBITBOXING_NAME",
                ChallengePinId = 2529088759,
                ChallengeRankPin = 958144818,
                ChallengeContentName = "TG_LittleBigPlanet2",
                ChallengePlanetUser = "qd3c781a5a6-GBen",
                ChallengePlanetId = 1085264,
                ChallengePhoto1 = 1112627,
            },
            new()
            {
                ChallengeId = 3,
                ChallengeActiveDateStarts = 1481155260000000,
                ChallengeActiveDateEnds = 1481414460000000,
                ChallengeLamsDescriptionId = "CHALLENGE_FLOATYFLUID_DESC",
                ChallengeLamsTitleId = "CHALLENGE_FLOATYFLUID_NAME",
                ChallengePinId = 183892581,
                ChallengeRankPin = 3442917932,
                ChallengeContent = "LBPDLCNISBLK0001",
                ChallengeContentName = "SBSP_THEME_PACK_NAME",
                ChallengePlanetUser = "qd3c781a5a6-GBen",
                ChallengePlanetId = 1095449,
                ChallengePhoto1 = 1112619,
            },
            new()
            {
                ChallengeId = 4,
                ChallengeActiveDateStarts = 1481414460000000,
                ChallengeActiveDateEnds = 1483574460000000,
                ChallengeLamsDescriptionId = "CHALLENGE_ISLANDRACE_DESC",
                ChallengeLamsTitleId = "CHALLENGE_ISLANDRACE_NAME",
                ChallengePinId = 315245769,
                ChallengeRankPin = 443310584,
                ChallengeContentName = "TG_LittleBigPlanet",
                ChallengePlanetUser = "qd3c781a5a6-GBen",
                ChallengePlanetId = 1102858,
                ChallengePhoto1 = 1112655,
            },
            new()
            {
                ChallengeId = 5,
                ChallengeActiveDateStarts = 1483574460000000,
                ChallengeActiveDateEnds = 1484784060000000,
                ChallengeLamsDescriptionId = "CHALLENGE_SPACEDODGING_DESC",
                ChallengeLamsTitleId = "CHALLENGE_SPACEDODGING_NAME",
                ChallengePinId = 144212050,
                ChallengeRankPin = 2123417147,
                ChallengeContentName = "TG_LittleBigPlanet3",
                ChallengePlanetUser = "qd3c781a5a6-GBen",
                ChallengePlanetId = 1085266,
                ChallengePhoto1 = 1112667,
            },
            new()
            {
                ChallengeId = 6,
                ChallengeActiveDateStarts = 1484784060000000,
                ChallengeActiveDateEnds = 1485993660000000,
                ChallengeLamsDescriptionId = "CHALLENGE_INVISIBLECIRCUIT_DESC",
                ChallengeLamsTitleId = "CHALLENGE_INVISIBLECIRCUIT_NAME",
                ChallengePinId = 249569175,
                ChallengeRankPin = 1943114258,
                ChallengeContentName = "TG_LittleBigPlanet",
                ChallengePlanetUser = "qd3c781a5a6-GBen",
                ChallengePlanetId = 1096814,
                ChallengePhoto1 = 1112635,
            },
            new()
            {
                ChallengeId = 7,
                ChallengeActiveDateStarts = 1485993660000000,
                ChallengeActiveDateEnds = 1487203260000000,
                ChallengeLamsDescriptionId = "CHALLENGE_HOVERBOARDRAILS_DESC",
                ChallengeLamsTitleId = "CHALLENGE_HOVERBOARDRAILS_NAME",
                ChallengePinId = 3478661003,
                ChallengeRankPin = 592022798,
                ChallengeContent = "LBPDLCBTTFLK0001",
                ChallengeContentName = "BTTF_LEVEL_KIT_NAME",
                ChallengePlanetUser = "qd3c781a5a6-GBen",
                ChallengePlanetId = 1085256,
                ChallengePhoto1 = 1112623,
            },
            new()
            {
                ChallengeId = 8,
                ChallengeActiveDateStarts = 1487203260000000,
                ChallengeActiveDateEnds = 1488412860000000,
                ChallengeLamsDescriptionId = "CHALLENGE_TOWERBOOST_DESC",
                ChallengeLamsTitleId = "CHALLENGE_TOWERBOOST_NAME",
                ChallengePinId = 216730878,
                ChallengeRankPin = 545532447,
                ChallengeContentName = "TG_LittleBigPlanet2",
                ChallengePlanetUser = "qd3c781a5a6-GBen",
                ChallengePlanetId = 1092504,
                ChallengePhoto1 = 1112671,
            },
            new()
            {
                ChallengeId = 9,
                ChallengeActiveDateStarts = 1488412860000000,
                ChallengeActiveDateEnds = 1489622460000000,
                ChallengeLamsDescriptionId = "CHALLENGE_SWOOPPANELS_DESC",
                ChallengeLamsTitleId = "CHALLENGE_SWOOPPANELS_NAME",
                ChallengePinId = 2054302637,
                ChallengeRankPin = 3288689476,
                ChallengeContentName = "TG_LittleBigPlanet2",
                ChallengePlanetUser = "qd3c781a5a6-GBen",
                ChallengePlanetId = 1085268,
                ChallengePhoto1 = 1112643,
            },
            new()
            {
                ChallengeId = 10,
                ChallengeActiveDateStarts = 1489622460000000,
                ChallengeActiveDateEnds = 1490832060000000,
                ChallengeLamsDescriptionId = "CHALLENGE_PINBALLCRYPTS_DESC",
                ChallengeLamsTitleId = "CHALLENGE_PINBALLCRYPTS_NAME",
                ChallengePinId = 618998172,
                ChallengeRankPin = 4087839785,
                ChallengeContentName = "TG_LittleBigPlanet3",
                ChallengePlanetUser = "qd3c781a5a6-GBen",
                ChallengePlanetId = 1085262,
                ChallengePhoto1 = 1112647,
            },
            new()
            {
                ChallengeId = 11,
                ChallengeActiveDateStarts = 1490832060000000,
                ChallengeActiveDateEnds = 1492041660000000,
                ChallengeLamsDescriptionId = "CHALLENGE_TIEHOP_DESC",
                ChallengeLamsTitleId = "CHALLENGE_TIEHOP_NAME",
                ChallengePinId = 3953447125,
                ChallengeRankPin = 2556445436,
                ChallengeContentName = "TG_LittleBigPlanet",
                ChallengePlanetUser = "qd3c781a5a6-GBen",
                ChallengePlanetId = 1092367,
                ChallengePhoto1 = 1112659,
            },
            new()
            {
                ChallengeId = 12,
                ChallengeActiveDateStarts = 1492041660000000,
                ChallengeActiveDateEnds = 1493251260000000,
                ChallengeLamsDescriptionId = "CHALLENGE_JOKERFUNHOUSE_DESC",
                ChallengeLamsTitleId = "CHALLENGE_JOKERFUNHOUSE_NAME",
                ChallengePinId = 1093784294,
                ChallengeRankPin = 1757295127,
                ChallengeContent = "LBPDLCWBDCLK0001",
                ChallengeContentName = "DCCOMICS_THEME_NAME",
                ChallengePlanetUser = "qd3c781a5a6-GBen",
                ChallengePlanetId = 1085258,
                ChallengePhoto1 = 1112631,
            },
            new()
            {
                ChallengeId = 13,
                ChallengeActiveDateStarts = 1493251260000000,
                ChallengeActiveDateEnds = 1494460860000000,
                ChallengeLamsDescriptionId = "CHALLENGE_DINERSHOOTING_DESC",
                ChallengeLamsTitleId = "CHALLENGE_DINERSHOOTING_NAME",
                ChallengePinId = 1568570416,
                ChallengeRankPin = 3721717765,
                ChallengeContentName = "TG_LittleBigPlanet3",
                ChallengePlanetUser = "qd3c781a5a6-GBen",
                ChallengePlanetId = 1085254,
                ChallengePhoto1 = 1112663,
            },
        },
    };

    #endregion
}

[XmlRoot("item_data")]
public class ChallengeItemData
{
    [XmlAttribute("Challenge_ID")]
    public int ChallengeId { get; set; }

    [XmlAttribute("Challenge_active_date_starts")]
    public ulong ChallengeActiveDateStarts { get; set; }

    [XmlAttribute("Challenge_active_date_ends")]
    public ulong ChallengeActiveDateEnds { get; set; }

    [XmlAttribute("Challenge_LAMSDescription_Id")]
    public string ChallengeLamsDescriptionId { get; set; }

    [XmlAttribute("Challenge_LAMSTitle_Id")]
    public string ChallengeLamsTitleId { get; set; }

    [XmlAttribute("Challenge_PinId")]
    public ulong ChallengePinId { get; set; }

    [XmlAttribute("Challenge_RankPin")]
    public ulong ChallengeRankPin { get; set; }

    [XmlAttribute("Challenge_Content")]
    public string ChallengeContent { get; set; }

    [XmlAttribute("Challenge_Content_name")]
    public string ChallengeContentName { get; set; }

    [XmlAttribute("Challenge_Planet_User")]
    public string ChallengePlanetUser { get; set; }

    [XmlAttribute("Challenge_planetId")]
    public ulong ChallengePlanetId { get; set; }

    [XmlAttribute("Challenge_photo_1")]
    public ulong ChallengePhoto1 { get; set; }
}