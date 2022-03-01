#nullable enable
using System.Linq;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Types.Match;

namespace LBPUnion.ProjectLighthouse.Types.Profiles;

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
        LastContact? lastContact = database.LastContacts.Where(l => l.UserId == userId).FirstOrDefault(l => TimestampHelper.Timestamp - l.Timestamp < 300);

        if (lastContact == null)
        {
            StatusType = StatusType.Offline;
            CurrentVersion = null;
        }
        else
        {
            StatusType = StatusType.Online;
            CurrentVersion = lastContact.GameVersion;
            CurrentPlatform = lastContact.Platform;
        }

        CurrentRoom = RoomHelper.FindRoomByUserId(userId);
    }

    public override string ToString()
    {
        CurrentVersion ??= GameVersion.Unknown;
        CurrentPlatform ??= Platform.Unknown;
        return this.StatusType switch
        {
            StatusType.Online => $"Currently online on {((GameVersion)this.CurrentVersion).ToPrettyString()} on {((Platform)this.CurrentPlatform)}",
            StatusType.Offline => "Offline",
            _ => "Unknown",
        };
    }
}