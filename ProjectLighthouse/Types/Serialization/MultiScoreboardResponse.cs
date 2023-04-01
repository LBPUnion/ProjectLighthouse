using System.Collections.Generic;
using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

[XmlRoot("scoreboards")]
public class MultiScoreboardResponse : ILbpSerializable
{
    public MultiScoreboardResponse() { }

    public MultiScoreboardResponse(List<PlayerScoreboardResponse> scoreboards)
    {
        this.Scoreboards = scoreboards;
    }

    [XmlElement("topScores")]
    public List<PlayerScoreboardResponse> Scoreboards { get; set; }
}

public class PlayerScoreboardResponse : ILbpSerializable
{
    public PlayerScoreboardResponse() { }

    public PlayerScoreboardResponse(List<GameScore> scores, int playerCount, int firstRank = 1)
    {
        this.Scores = scores;
        this.PlayerCount = playerCount;
        this.FirstRank = firstRank;
    }

    [XmlElement("playRecord")]
    public List<GameScore> Scores { get; set; }

    [XmlAttribute("players")]
    public int PlayerCount { get; set; }

    [XmlAttribute("firstRank")]
    public int FirstRank { get; set; }

}