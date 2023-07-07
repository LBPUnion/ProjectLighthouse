using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Localization;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;

public class BaseFrame : PageModel
{
    public readonly DatabaseContext Database;

    public BaseFrame(DatabaseContext database)
    {
        this.Database = database;
    }

    public string Title = string.Empty;
    public bool IsMobile;

    private UserEntity? user;
    public new UserEntity? User
    {
        get
        {
            if (this.user != null) return this.user;

            return this.user = this.Database.UserFromWebRequest(this.Request);
        }
        set => this.user = value;
    }

    private string? language;
    private string? timeZone;

    public string GetLanguage()
    {
        if (ServerStatics.IsUnitTesting) return LocalizationManager.DefaultLang;
        if (this.language != null) return this.language;

        if (this.User != null) return this.language = this.User.Language;

        IRequestCultureFeature? requestCulture = this.Request.HttpContext.Features.Get<IRequestCultureFeature>();
        if (requestCulture == null) return this.language = LocalizationManager.DefaultLang;

        return this.language = requestCulture.RequestCulture.UICulture.Name;
    }

    public string GetTimeZone()
    {
        if (ServerStatics.IsUnitTesting) return TimeZoneInfo.Local.Id;
        if (this.timeZone != null) return this.timeZone;

        string userTimeZone = this.User?.TimeZone ?? TimeZoneInfo.Local.Id;

        return this.timeZone = userTimeZone;
    }
}