using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

[XmlRoot("reviews")]
public struct ReviewResponse : ILbpSerializable
{
    public ReviewResponse(List<GameReview> reviews, long hint, int hintStart)
    {
        this.Reviews = reviews;
        this.Hint = hint;
        this.HintStart = hintStart;
    }

    [XmlElement("review")]
    public List<GameReview> Reviews { get; set; }

    [DefaultValue(0)]
    [XmlAttribute("hint")]
    public long Hint { get; set; }

    [DefaultValue(0)]
    [XmlAttribute("hint_start")]
    public int HintStart { get; set; }
}