using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Serialization;

#nullable enable
public class News {
    [Key]
    public int NewsId { get; set; }
    public string Category { get; set; } = "";
    public string Title { get; set; } = "";
    public string Summary { get; set; } = "";
    public string Text { get; set; } = "";
    public long Timestamp {get; set; }

    public int CreatorId { get; set; }
    
    public string ImageAlign { get; set;} = "";
    public string ImageHash { get; set; } = "";

    public string Serialize()
    {
        string newsData = LbpSerializer.StringElement("id", this.NewsId) +
                          LbpSerializer.StringElement("category", this.Category) +
                          LbpSerializer.StringElement("title", this.Title) +
                          LbpSerializer.StringElement("summary", this.Summary) +
                          LbpSerializer.StringElement("text", this.Text) +
                          LbpSerializer.StringElement("date", this.Timestamp) +
                          LbpSerializer.StringElement("image", 
                            LbpSerializer.StringElement("alignment", this.ImageAlign) +
                            LbpSerializer.StringElement("hash", this.ImageHash)
                          );
        return LbpSerializer.StringElement("item", newsData);
    }
}