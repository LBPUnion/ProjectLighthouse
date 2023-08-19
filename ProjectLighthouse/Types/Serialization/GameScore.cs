using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

[XmlRoot("playRecord")]
[XmlType("playRecord")]
public class GameScore : ILbpSerializable, INeedsPreparationForSerialization
{
    [XmlIgnore]
    public int UserId { get; set; }

    [XmlElement("type")]
    public int Type { get; set; }

    [DefaultValue(null)]
    [XmlElement("playerIds")]
    public string[] PlayerIds;

    [DefaultValue("")]
    [XmlElement("mainPlayer")]
    public string MainPlayer { get; set; }

    [DefaultValue(0)]
    [XmlElement("rank")]
    public int Rank { get; set; }

    [XmlElement("score")]
    public int Points { get; set; }

    public async Task PrepareSerialization(DatabaseContext database)
    {
        this.MainPlayer = await database.Users.Where(u => u.UserId == this.UserId)
            .Select(u => u.Username)
            .FirstAsync();
    }

    public static GameScore CreateFromEntity(ScoreEntity entity, int rank) =>
        new()
        {
            UserId = entity.UserId,
            Points = entity.Points,
            Type = entity.Type,
            Rank = rank,
        };
}