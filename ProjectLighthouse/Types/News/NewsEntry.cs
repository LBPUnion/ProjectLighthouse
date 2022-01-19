using LBPUnion.ProjectLighthouse.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.News;

/// <summary>
///     Used on the info moon on LBP1. Broken for unknown reasons
/// </summary>
public class NewsEntry
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Summary { get; set; }
    public string Text { get; set; }
    public NewsImage Image { get; set; }
    public string Category { get; set; }
    public long Date { get; set; }

    public string Serialize()
        => LbpSerializer.StringElement("id", this.Id) +
           LbpSerializer.StringElement("title", this.Title) +
           LbpSerializer.StringElement("summary", this.Summary) +
           LbpSerializer.StringElement("text", this.Text) +
           LbpSerializer.StringElement("date", this.Date) +
           this.Image.Serialize() +
           LbpSerializer.StringElement("category", this.Category);
}