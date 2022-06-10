#nullable enable
using System.Linq;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Match.Rooms;

namespace LBPUnion.ProjectLighthouse.PlayerData.Profiles;

public class UserStatus
{
    public StatusType StatusType { get; set; }
    public GameVersion? CurrentVersion { get; set; }
    public Platform? CurrentPlatform { get; set; }
    public Room? CurrentRoom { get; set; }

    public UserStatus()
    {}

    public UserStatus(Database database, int userId)
    {
        LastContact? lastContact = database.LastContacts.Where(l => l.UserId == userId).FirstOrDefault(l => TimeHelper.Timestamp - l.Timestamp < 300);

        if (lastContact == null)
        {
            this.StatusType = StatusType.Offline;
            this.CurrentVersion = null;
        }
        else
        {
            this.StatusType = StatusType.Online;
            this.CurrentVersion = lastContact.GameVersion;
            this.CurrentPlatform = lastContact.Platform;
        }

        this.CurrentRoom = RoomHelper.FindRoomByUserId(userId);
    }

    public override string ToString()
    {
        this.CurrentVersion ??= GameVersion.Unknown;
        this.CurrentPlatform ??= Platform.Unknown;
        return this.StatusType switch
        {
            StatusType.Online => $"Currently online on {((GameVersion)this.CurrentVersion).ToPrettyString()} on {((Platform)this.CurrentPlatform)}",
            StatusType.Offline => "Offline",
            _ => "Unknown",
        };
    }
}