using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Misc;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

[XmlRoot("user")]
public class GameUser : ILbpSerializable, INeedsPreparationForSerialization
{
    [XmlIgnore]
    public int UserId { get; set; }

    [XmlIgnore]
    public GameVersion TargetGame { get; set; }

    [XmlIgnore]
    public string PlanetHashLBP2 { get; set; }

    [XmlIgnore]
    public string PlanetHashLBP3 { get; set; }

    [XmlIgnore]
    public string PlanetHashLBPVita { get; set; }

    [XmlElement("npHandle")]
    public NpHandle UserHandle { get; set; } = new();

    [XmlElement("game")]
    public int Game { get; set; }

    [DefaultValue(0)]
    [XmlElement("lists")]
    public int PlaylistCount { get; set; }

    [DefaultValue(0)]
    [XmlElement("lists_quota")]
    public int PlaylistQuota { get; set; } = ServerConfiguration.Instance.UserGeneratedContentLimits.ListsQuota;

    [DefaultValue(0)]
    [XmlElement("heartCount")]
    public int HeartCount { get; set; }

    [DefaultValue("")]
    [XmlElement("yay2")]
    public string YayHash { get; set; }

    [DefaultValue("")]
    [XmlElement("meh2")]
    public string MehHash { get; set; }

    [DefaultValue("")]
    [XmlElement("boo2")]
    public string BooHash { get; set; }

    [DefaultValue("")]
    [XmlElement("biography")]
    public string Biography { get; set; }

    [DefaultValue(0)]
    [XmlElement("reviewCount")]
    public int ReviewCount { get; set; }

    [DefaultValue(0)]
    [XmlElement("commentCount")]
    public int CommentCount { get; set; }

    [DefaultValue(0)]
    [XmlElement("photosByMeCount")]
    public int PhotosByMeCount { get; set; }

    [DefaultValue(0)]
    [XmlElement("photosWithMeCount")]
    public int PhotosWithMeCount { get; set; }

    [XmlElement("commentsEnabled")]
    public bool CommentsEnabled { get; set; }

    [XmlElement("location")]
    public Location Location { get; set; } = new();

    [DefaultValue(0)]
    [XmlElement("favouriteSlotCount")]
    public int HeartedLevelCount { get; set; }

    [DefaultValue(0)]
    [XmlElement("favouriteUserCount")]
    public int HeartedUserCount { get; set; }

    [DefaultValue(0)]
    [XmlElement("favouritePlaylistCount")]
    public int HeartedPlaylistCount { get; set; }

    [DefaultValue(0)]
    [XmlElement("lolcatftwCount")]
    public int QueuedLevelCount { get; set; }

    [DefaultValue("")]
    [XmlElement("pins")]
    public string ProfilePins { get; set; }

    #region Planets
    [DefaultValue("")]
    [XmlElement("planets")]
    public string PlanetHash { get; set; }

    [DefaultValue("")]
    [XmlElement("crossControlPlanet")]
    public string PlanetHashLBP2CC { get; set; }
    public bool ShouldSerializePlanetHashLBP2CC() => this.TargetGame == GameVersion.LittleBigPlanet2;
    #endregion

    #region Used Slots

    // Used to identify LBP1 used slots in LBP2 and beyond
    [DefaultValue(0)]
    [XmlElement("lbp1UsedSlots")]
    public int Lbp1UsedSlots { get; set; }

    // Used to calculate the number of slots the user has used in LBP1 only
    [DefaultValue(0)]
    [XmlElement("freeSlots")]
    public int Lbp1FreeSlots { get; set; }

    [DefaultValue(0)]
    [XmlElement("entitledSlots")]
    public int Lbp1EntitledSlots { get; set; }

    [DefaultValue(0)]
    [XmlElement("lbp2UsedSlots")]
    public int Lbp2UsedSlots { get; set; }

    [DefaultValue(0)]
    [XmlElement("lbp2EntitledSlots")]
    public int Lbp2EntitledSlots { get; set; }

    [DefaultValue(0)]
    [XmlElement("crossControlEntitledSlots")]
    public int CrossControlEntitledSlots { get; set; }

    [DefaultValue(0)]
    [XmlElement("crossControlUsedSlots")]
    public int CrossControlUsedSlots { get; set; }

    [DefaultValue(0)]
    [XmlElement("lbp3UsedSlots")]
    public int Lbp3UsedSlots { get; set; }

    [DefaultValue(0)]
    [XmlElement("lbp3EntitledSlots")]
    public int Lbp3EntitledSlots { get; set; }

    #endregion

    public async Task PrepareSerialization(DatabaseContext database)
    {
        var stats = await database.Users.Where(u => u.UserId == this.UserId)
            .Select(_ => new
            {
                BonusSlots = database.Users.Where(u => u.UserId == this.UserId).Select(u => u.AdminGrantedSlots).First(),
                PlaylistCount = database.Playlists.Count(p => p.CreatorId == this.UserId),
                ReviewCount = database.Reviews.Count(r => r.ReviewerId == this.UserId),
                CommentCount = database.Comments.Count(c => c.TargetUserId == this.UserId),
                HeartCount = database.HeartedProfiles.Count(h => h.HeartedUserId == this.UserId),
                PhotosByMeCount = database.Photos.Count(p => p.CreatorId == this.UserId),
                PhotosWithMeCount = database.Photos.Include(p => p.PhotoSubjects)
                    .Count(p => p.PhotoSubjects.Any(ps => ps.UserId == this.UserId)),
                HeartedLevelCount = database.HeartedLevels.Count(h => h.UserId == this.UserId),
                HeartedUserCount = database.HeartedProfiles.Count(h => h.UserId == this.UserId),
                HeartedPlaylistCount = database.HeartedPlaylists.Count(h => h.UserId == this.UserId),
                QueuedLevelCount = database.QueuedLevels.Count(q => q.UserId == this.UserId),
            })
            .OrderBy(_ => 1)
            .FirstAsync();

        this.CommentsEnabled = this.CommentsEnabled && ServerConfiguration.Instance.UserGeneratedContentLimits.ProfileCommentsEnabled;

        int entitledSlots = ServerConfiguration.Instance.UserGeneratedContentLimits.EntitledSlots + stats.BonusSlots;

        IQueryable<SlotEntity> SlotCount(GameVersion version)
        {
            return database.Slots.Where(s => s.CreatorId == this.UserId && s.GameVersion == version);
        }

        if (this.TargetGame == GameVersion.LittleBigPlanetVita)
        {
            this.Lbp2EntitledSlots = entitledSlots;
            this.Lbp2UsedSlots = await SlotCount(GameVersion.LittleBigPlanetVita).CountAsync();
        }
        else
        {
            this.Lbp1EntitledSlots = entitledSlots;
            this.Lbp2EntitledSlots = entitledSlots;
            this.CrossControlEntitledSlots = entitledSlots;
            this.Lbp3EntitledSlots = entitledSlots;
            this.Lbp1UsedSlots = await SlotCount(GameVersion.LittleBigPlanet1).CountAsync();
            this.Lbp2UsedSlots = await SlotCount(GameVersion.LittleBigPlanet2).CountAsync(s => !s.CrossControllerRequired);
            this.Lbp3UsedSlots = await SlotCount(GameVersion.LittleBigPlanet3).CountAsync();
            
            this.Lbp1FreeSlots = this.Lbp1EntitledSlots - this.Lbp1UsedSlots;

            this.CrossControlUsedSlots = await database.Slots.CountAsync(s => s.CreatorId == this.UserId && s.CrossControllerRequired);
        }

        this.Game = (int)this.TargetGame;

        this.PlanetHash = this.TargetGame switch
        {
            GameVersion.LittleBigPlanet2 => this.PlanetHashLBP2,
            GameVersion.LittleBigPlanet3 => this.PlanetHashLBP3,
            GameVersion.LittleBigPlanetVita => this.PlanetHashLBPVita,
            _ => "", // other versions do not have custom planets
        };

        ReflectionHelper.CopyAllFields(stats, this);
    }

    public static GameUser CreateFromEntity(UserEntity entity, GameVersion targetGame)
    {
        GameUser profile = CreateFromEntity(entity);
        profile.TargetGame = targetGame;
        return profile;
    }

    public static GameUser CreateFromEntity(UserEntity entity) =>
        new()
        {
            UserId = entity.UserId,
            UserHandle = new NpHandle(entity.Username, entity.IconHash),
            Biography = entity.Biography,
            Location = entity.Location,
            ProfilePins = entity.Pins,
            YayHash = entity.YayHash,
            MehHash = entity.MehHash,
            BooHash = entity.BooHash,
            CommentsEnabled = entity.CommentsEnabled,
            PlanetHashLBP2 = entity.PlanetHashLBP2,
            PlanetHashLBP2CC = entity.PlanetHashLBP2CC,
            PlanetHashLBP3 = entity.PlanetHashLBP3,
            PlanetHashLBPVita = entity.PlanetHashLBPVita,
        };
}