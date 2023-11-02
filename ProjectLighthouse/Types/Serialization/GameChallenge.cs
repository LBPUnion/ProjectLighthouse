using System.Collections.Generic;
using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

[XmlRoot("Challenge_header")]
public class GameChallenge
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
    public List<ChallengeItemData> ChallengeItemDatas { get; set; }
}

[XmlRoot("item_data")]
public class ChallengeItemData
{
    [XmlAttribute("Challenge_ID")]
    public int ChallengeId { get; set; }

    [XmlAttribute("Challenge_active_date_starts")]
    public int ChallengeActiveDateStarts { get; set; }

    [XmlAttribute("Challenge_active_date_ends")]
    public int ChallengeActiveDateEnds { get; set; }

    [XmlAttribute("Challenge_LAMSDescription_Id")]
    public int ChallengeLamsDescriptionId { get; set; }

    [XmlAttribute("Challenge_LAMSTitle_Id")]
    public int ChallengeLamsTitleId { get; set; }

    [XmlAttribute("Challenge_PinId")]
    public int ChallengePinId { get; set; }

    [XmlAttribute("Challenge_RankPin")]
    public int ChallengeRankPin { get; set; }

    [XmlAttribute("Challenge_Content_name")]
    public int ChallengeContentName { get; set; }

    [XmlAttribute("Challenge_Planet_User")]
    public int ChallengePlanetUser { get; set; }

    [XmlAttribute("Challenge_planetId")]
    public int ChallengePlanetId { get; set; }

    [XmlAttribute("Challenge_photo_1")]
    public int ChallengePhoto1 { get; set; }
}