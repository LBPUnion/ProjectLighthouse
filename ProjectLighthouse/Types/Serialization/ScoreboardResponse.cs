using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

public struct ScoreboardResponse: ILbpSerializable, IHasCustomRoot
{
    public ScoreboardResponse() { }

    public ScoreboardResponse(string rootElement, List<GameScore> scores, int total, int yourScore, int yourRank)
    {
        this.RootTag = rootElement;
        this.Scores = scores;
        this.Total = total;
        this.YourScore = yourScore;
        this.YourRank = yourRank;
    }

    public ScoreboardResponse(List<GameScore> scores, int total, int yourScore, int yourRank) : this("scores", scores, total, yourScore, yourRank)
    { }

    [XmlIgnore]
    public string RootTag { get; set; }

    [XmlElement("playRecord")]
    public List<GameScore> Scores { get; set; }

    [DefaultValue(0)]
    [XmlAttribute("totalNumScores")]
    public int Total { get; set; }

    [DefaultValue(0)]
    [XmlAttribute("yourScore")]
    public int YourScore { get; set; }

    [DefaultValue(0)]
    [XmlAttribute("yourRank")]
    public int YourRank { get; set; }

    public string GetRoot() => this.RootTag;
    
}