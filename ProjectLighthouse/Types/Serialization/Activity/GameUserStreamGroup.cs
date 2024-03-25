using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;

namespace LBPUnion.ProjectLighthouse.Types.Serialization.Activity;

public class GameUserStreamGroup : GameStreamGroup, INeedsPreparationForSerialization
{
    [XmlIgnore]
    public int UserId { get; set; }

    [XmlElement("user_id")]
    public string Username { get; set; }

    public async Task PrepareSerialization(DatabaseContext database)
    {
        UserEntity user = await database.Users.FindAsync(this.UserId);
        if (user == null) return;

        this.Username = user.Username;
    }

    public static GameUserStreamGroup Create(int userId) =>
        new()
        {
            UserId = userId,
        };
}