#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Serialization;

namespace LBPUnion.ProjectLighthouse.Levels;

public class Playlist
{
    [Key]
    public int PlaylistId { get; set; }

    [XmlElement("name")]
    public string Name { get; set; } = "";

    [XmlElement("description")]
    public string Description { get; set; } = "";

    public int CreatorId { get; set; }

    [ForeignKey(nameof(CreatorId))]
    [JsonIgnore]
    public User? Creator { get; set; }

    public string SlotCollection { get; set; } = "";

    [JsonIgnore]
    [NotMapped]
    [XmlElement("level_id")]
    public int[]? LevelIds { get; set; }

    [NotMapped]
    public int[] SlotIds
    {
        get => this.SlotCollection.Split(",").Where(x => int.TryParse(x, out _)).Select(int.Parse).ToArray();
        set => this.SlotCollection = string.Join(",", value);
    }

    public string Serialize()
    {
        using Database database = new();
        string playlist = LbpSerializer.StringElement("id", this.PlaylistId) +
                          LbpSerializer.StringElement("author",
                              LbpSerializer.StringElement("npHandle", this.Creator?.Username)) +
                          LbpSerializer.StringElement("name", this.Name) +
                          LbpSerializer.StringElement("description", this.Description) +
                          LbpSerializer.StringElement("levels", this.SlotIds.Length) +
                          LbpSerializer.StringElement("thumbs", 0) +
                          LbpSerializer.StringElement("plays", 0) +
                          LbpSerializer.StringElement("unique_plays", 0) +
                          LbpSerializer.StringElement("levels_quota", ServerConfiguration.Instance.UserGeneratedContentLimits.ListsQuota) +
                          this.SerializeIcons(database);
        
        return LbpSerializer.StringElement("playlist", playlist);
    }

    private string SerializeIcons(Database database)
    {
        string iconList = this.SlotIds.Select(id => database.Slots.FirstOrDefault(s => s.SlotId == id))
            .Where(slot => slot != null)
            .Aggregate(string.Empty, (current, slot) => current + LbpSerializer.StringElement("icon", slot?.IconHash));
        return LbpSerializer.StringElement("icons", iconList);
    }

}