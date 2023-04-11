#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

[XmlRoot("playlist")]
public class GamePlaylist : ILbpSerializable, INeedsPreparationForSerialization
{
    [XmlIgnore]
    public int CreatorId { get; set; }

    [XmlIgnore]
    public int[] SlotIds { get; set; } = Array.Empty<int>();

    [XmlElement("id")]
    public int PlaylistId { get; set; }

    [XmlElement("author")]
    public Author AuthorHandle { get; set; } = new();

    [XmlElement("name")]
    public string Name { get; set; } = "";

    [DefaultValue("")]
    [XmlElement("description")]
    public string Description { get; set; } = "";

    [DefaultValue(0)]
    [XmlElement("levels")]
    public int LevelCount { get; set; }

    [DefaultValue(0)]
    [XmlElement("hearts")]
    public int Hearts { get; set; }

    [XmlElement("levels_quota")]
    public int PlaylistQuota { get; set; }

    [XmlElement("level_id")]
    public int[]? LevelIds { get; set; }
    public bool ShouldSerializeLevelIds() => false;

    [XmlElement("icons")]
    public IconList LevelIcons { get; set; } = new();

    public async Task PrepareSerialization(DatabaseContext database)
    {
        string authorUsername = await database.Users.Where(u => u.UserId == this.CreatorId)
                .Select(u => u.Username)
                .FirstAsync();
        this.AuthorHandle = new Author
        {
            Username = authorUsername,
        };

        this.Hearts = await database.HeartedPlaylists.CountAsync(h => h.HeartedPlaylistId == this.PlaylistId);
        this.PlaylistQuota = ServerConfiguration.Instance.UserGeneratedContentLimits.ListsQuota;
        List<string> iconList = this.SlotIds.Select(id => database.Slots.FirstOrDefault(s => s.SlotId == id))
            .Where(slot => slot != null)
            .Where(slot => slot!.IconHash.Length > 0)
            .Select(slot => slot!.IconHash)
            .ToList();

        this.LevelIcons = new IconList(iconList);
    }

    public static GamePlaylist CreateFromEntity(PlaylistEntity playlist) =>
        new()
        {
            Name = playlist.Name,
            Description = playlist.Description,
            SlotIds = playlist.SlotIds,
            CreatorId = playlist.CreatorId,
            PlaylistId = playlist.PlaylistId,
        };
}