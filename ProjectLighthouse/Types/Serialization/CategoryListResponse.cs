using System.Collections.Generic;
using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

[XmlRoot("categories")]
public class CategoryListResponse : ILbpSerializable
{
    public CategoryListResponse() { }

    public CategoryListResponse(List<GameCategory> categories, int total, int hint, int hintStart)
    {
        this.Categories = categories;
        this.Total = total;
        this.Hint = hint;
        this.HintStart = hintStart;
    }

    [XmlAttribute("total")]
    public int Total { get; set; }

    [XmlAttribute("hint")]
    public int Hint { get; set; }

    [XmlAttribute("hint_start")]
    public int HintStart { get; set; }

    [XmlElement("text_search")]
    public TextSearch Search { get; set; } = new();

    [XmlElement("category")]
    public List<GameCategory> Categories { get; set; }
}