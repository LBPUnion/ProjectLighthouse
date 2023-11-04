using System.Collections.Generic;
using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

[XmlRoot("Challenge_header")]
public partial class GameChallengeResponse : ILbpSerializable
{
    [XmlElement("Total_challenges")]
    public int TotalChallenges { get; set; }

    [XmlElement("Challenge_End_Date")]
    public ulong EndTime { get; set; }

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

}

public class GameChallenge
{
    [XmlAttribute("Challenge_ID")]
    public int Id { get; set; }

    [XmlAttribute("Challenge_active_date_starts")]
    public ulong StartTime { get; set; }

    [XmlAttribute("Challenge_active_date_ends")]
    public ulong EndTime { get; set; }

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
    public ulong PhotoId { get; set; }
}