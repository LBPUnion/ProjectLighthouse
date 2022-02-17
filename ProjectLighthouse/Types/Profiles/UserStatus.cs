#nullable enable
using System.Linq;
using LBPUnion.ProjectLighthouse.Helpers;

namespace LBPUnion.ProjectLighthouse.Types.Profiles;

public class UserStatus
{
    public StatusType StatusType { get; set; }
    public GameVersion? CurrentVersion { get; set; }

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
        }
    }

    public override string ToString()
    {
        CurrentVersion ??= GameVersion.Unknown;

        return this.StatusType switch
        {
            StatusType.Online => $"Currently online on {((GameVersion)this.CurrentVersion).ToPrettyString()}",
            StatusType.Offline => "Offline",
            _ => "Unknown",
        };
    }
}