#nullable enable
using System;
using System.Linq;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Localization.StringLists;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Matchmaking.Rooms;

namespace LBPUnion.ProjectLighthouse.Types.Users;

public enum StatusType
{
    Offline = 0,
    Online = 1,
}

public class UserStatus
{
    public StatusType StatusType { get; set; }
    public GameVersion? CurrentVersion { get; set; }
    public Platform? CurrentPlatform { get; set; }
    public Room? CurrentRoom { get; set; }
    public long LastLogin { get; set; }
    public long LastLogout { get; set; }

    public UserStatus()
    {}

    public UserStatus(DatabaseContext database, int userId)
    {
        LastContactEntity? lastContact = database.LastContacts.Where(l => l.UserId == userId).FirstOrDefault(l => TimeHelper.Timestamp - l.Timestamp < 300);

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
        this.LastLogin = loginTimestamps?.LastLogin ?? 0;
        this.LastLogout = loginTimestamps?.LastLogout ?? 0;

       this.CurrentRoom = RoomHelper.FindRoomByUserId(userId);
    }

    private string FormatOfflineTimestamp(string language, string timeZone)
    {
        if (this.LastLogout <= 0 && this.LastLogin <= 0)
        {
            return StatusStrings.Offline.Translate(language);
        }

        long timestamp = this.LastLogout;
        if (timestamp <= 0) timestamp = this.LastLogin;
        TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
        string formattedTime = TimeZoneInfo.ConvertTime(DateTimeOffset.FromUnixTimeMilliseconds(timestamp), timeZoneInfo).ToString("M/d/yyyy h:mm:ss tt");
        return StatusStrings.LastOnline.Translate(language, formattedTime);
    }

    public string ToTranslatedString(string language, string timeZone)
    {
        this.CurrentVersion ??= GameVersion.Unknown;
        this.CurrentPlatform ??= Platform.Unknown;

        return this.StatusType switch
        {
            StatusType.Online => StatusStrings.CurrentlyOnline.Translate(language, 
                ((GameVersion)this.CurrentVersion).ToPrettyString(), (Platform)this.CurrentPlatform),
            StatusType.Offline => this.FormatOfflineTimestamp(language, timeZone),
            _ => GeneralStrings.Unknown.Translate(language),
        };
    }
}