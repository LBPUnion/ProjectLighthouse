using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Activity;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Entities.Website;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Serialization.News;
using LBPUnion.ProjectLighthouse.Types.Serialization.Playlist;
using LBPUnion.ProjectLighthouse.Types.Serialization.Slot;
using LBPUnion.ProjectLighthouse.Types.Serialization.User;
using LBPUnion.ProjectLighthouse.Types.Users;

namespace LBPUnion.ProjectLighthouse.Types.Serialization.Activity;

/// <summary>
/// The global stream object, contains all 
/// </summary>
[XmlRoot("stream")]
public class GameStream : ILbpSerializable, INeedsPreparationForSerialization
{
    /// <summary>
    /// A list of <see cref="SlotEntity.SlotId"/> that should be included in the root
    /// of the stream object. These will be loaded into <see cref="Slots"/> 
    /// </summary>
    [XmlIgnore]
    public List<int> SlotIds { get; set; }

    /// <summary>
    /// A list of <see cref="UserEntity.UserId"/> that should be included in the root
    /// of the stream object. These will be loaded into <see cref="Users"/> 
    /// </summary>
    [XmlIgnore]
    public List<int> UserIds { get; set; }

    /// <summary>
    /// A list of <see cref="PlaylistEntity.PlaylistId"/> that should be included in the root
    /// of the stream object. These will be loaded into <see cref="Playlists"/> 
    /// </summary>
    [XmlIgnore]
    public List<int> PlaylistIds { get; set; }

    /// <summary>
    /// A list of <see cref="WebsiteAnnouncementEntity.AnnouncementId"/> that should be included in the root
    /// of the stream object. These will be loaded into <see cref="News"/> 
    /// </summary>
    [XmlIgnore]
    public List<int> NewsIds { get; set; }

    [XmlIgnore]
    private GameVersion TargetGame { get; set; }

    [XmlElement("start_timestamp")]
    public long StartTimestamp { get; set; }

    [XmlElement("end_timestamp")]
    public long EndTimestamp { get; set; }

    [XmlArray("groups")]
    [XmlArrayItem("group")]
    [DefaultValue(null)]
    public List<GameStreamGroup> Groups { get; set; }

    [XmlArray("slots")]
    [XmlArrayItem("slot")]
    [DefaultValue(null)]
    public List<SlotBase> Slots { get; set; }

    [XmlArray("users")]
    [XmlArrayItem("user")]
    [DefaultValue(null)]
    public List<GameUser> Users { get; set; }

    [XmlArray("playlists")]
    [XmlArrayItem("playlist")]
    [DefaultValue(null)]
    public List<GamePlaylist> Playlists { get; set; }

    [XmlArray("news")]
    [XmlArrayItem("item")]
    [DefaultValue(null)]
    public List<GameNewsObject> News { get; set; }

    public async Task PrepareSerialization(DatabaseContext database)
    {
        this.Slots = await LoadEntities<SlotEntity, SlotBase>(this.SlotIds, slot => SlotBase.CreateFromEntity(slot, this.TargetGame, 0), s => s.Type == SlotType.User);
        this.Users = await LoadEntities<UserEntity, GameUser>(this.UserIds, user => GameUser.CreateFromEntity(user, this.TargetGame));
        this.Playlists = await LoadEntities<PlaylistEntity, GamePlaylist>(this.PlaylistIds, GamePlaylist.CreateFromEntity);
        this.News = await LoadEntities<WebsiteAnnouncementEntity, GameNewsObject>(this.NewsIds, a => GameNewsObject.CreateFromEntity(a, this.TargetGame));
        return;

        async Task<List<TResult>> LoadEntities<TFrom, TResult>(List<int> ids, Func<TFrom, TResult> transformation, Func<TFrom, bool> predicate = null) 
            where TFrom : class
        {
            List<TResult> results = [];
            if (ids.Count <= 0) return null;
            foreach (int id in ids)
            {
                TFrom entity = await database.Set<TFrom>().FindAsync(id);

                if (entity == null) continue;

                if (predicate != null && !predicate(entity)) continue;

                results.Add(transformation(entity));
            }

            return results;
        }
    }

    public static GameStream CreateFromGroups
        (GameTokenEntity token, List<OuterActivityGroup> groups, long startTimestamp, long endTimestamp, bool removeNesting = false)
    {
        GameStream gameStream = new()
        {
            TargetGame = token.GameVersion,
            StartTimestamp = startTimestamp,
            EndTimestamp = endTimestamp,
            SlotIds = groups.GetIds(ActivityGroupType.Level),
            UserIds = groups.GetIds(ActivityGroupType.User),
            PlaylistIds = groups.GetIds(ActivityGroupType.Playlist),
            NewsIds = groups.GetIds(ActivityGroupType.News),
        };
        if (groups.Count == 0) return gameStream;

        gameStream.Groups = groups.Select(GameStreamGroup.CreateFromGroup).ToList();

        // Workaround for level activity because it shouldn't contain nested activity groups
        if (gameStream.Groups.Count == 1 && groups.First().Key.GroupType == ActivityGroupType.Level && removeNesting)
        {
            gameStream.Groups = gameStream.Groups.First().Groups;
        }

        // Workaround to turn a single subgroup into the primary group for news and team picks
        for (int i = 0; i < gameStream.Groups.Count; i++)
        {
            GameStreamGroup group = gameStream.Groups[i];
            if (group.Type is not (ActivityGroupType.TeamPick or ActivityGroupType.News)) continue;
            if (group.Groups.Count > 1) continue;

            gameStream.Groups[i] = group.Groups.First();
        }

        return gameStream;
    }
}