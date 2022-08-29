#nullable enable
using System;
using System.Linq;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Localization.StringLists;
using LBPUnion.ProjectLighthouse.Match.Rooms;

namespace LBPUnion.ProjectLighthouse.PlayerData.Profiles;

public class UserStatus
{
    public StatusType StatusType { get; set; }
    public GameVersion? CurrentVersion { get; set; }
    public Platform? CurrentPlatform { get; set; }
    public Room? CurrentRoom { get; set; }
    public long LastLogin { get; set; } = -1;
    public long LastLogout { get; set; } = -1;

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

        var loginTimestamps = database.Users.Where(u => u.UserId == userId)
            .Select(u => new
            {
                u.LastLogin,
                u.LastLogout,
            }).FirstOrDefault();
        this.LastLogin = loginTimestamps?.LastLogin ?? -1;
        this.LastLogout = loginTimestamps?.LastLogout ?? -1;

       this.CurrentRoom = RoomHelper.FindRoomByUserId(userId);
    }

    private string FormatOfflineTimestamp(string language)
    {
        if (this.LastLogout <= 0 && this.LastLogin <= 0)
        {
            return StatusStrings.Offline.Translate(language);
        }

        long timestamp = this.LastLogout;
        if (timestamp <= 0) timestamp = this.LastLogin;
        string formattedTime = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).ToLocalTime().ToString("M/d/yyyy h:mm:ss tt");
        return StatusStrings.LastOnline.Translate(language, formattedTime);
    }

    public string ToTranslatedString(string language)
    {
        this.CurrentVersion ??= GameVersion.Unknown;
        this.CurrentPlatform ??= Platform.Unknown;

        return this.StatusType switch
        {
            StatusType.Online => StatusStrings.CurrentlyOnline.Translate(language, 
                ((GameVersion)this.CurrentVersion).ToPrettyString(), (Platform)this.CurrentPlatform),
            StatusType.Offline => this.FormatOfflineTimestamp(language),
            _ => GeneralStrings.Unknown.Translate(language),
        };
    }
}