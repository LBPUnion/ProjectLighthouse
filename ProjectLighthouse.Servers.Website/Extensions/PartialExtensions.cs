using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Extensions;

public static class PartialExtensions
{

    public static ViewDataDictionary<T> WithLang<T>(this ViewDataDictionary<T> viewData, string language) => WithKeyValue(viewData, "Language", language);

    public static ViewDataDictionary<T> WithTime<T>(this ViewDataDictionary<T> viewData, string timeZone) => WithKeyValue(viewData, "TimeZone", timeZone);

    public static ViewDataDictionary<T> CanDelete<T>(this ViewDataDictionary<T> viewData, bool canDelete) => WithKeyValue(viewData, "CanDelete", canDelete);

    private static ViewDataDictionary<T> WithKeyValue<T>(this ViewDataDictionary<T> viewData, string key, object? value)
    {
        try
        {
            return new ViewDataDictionary<T>(viewData)
            {
                {
                    key, value
                },
            };
        }
        catch
        {
            return viewData;
        }
    }

    public static Task<IHtmlContent> ToLink<T>(this User user, IHtmlHelper<T> helper, ViewDataDictionary<T> viewData, string language, string timeZone = "", bool includeStatus = false) 
        => helper.PartialAsync("Partials/Links/UserLinkPartial", user, viewData.WithLang(language).WithTime(timeZone).WithKeyValue("IncludeStatus", includeStatus));

    public static Task<IHtmlContent> ToHtml<T>
    (
        this Slot slot,
        IHtmlHelper<T> helper,
        ViewDataDictionary<T> viewData,
        User? user,
        string callbackUrl,
        string language = "",
        string timeZone = "",
        bool isMobile = false,
        bool showLink = false,
        bool isMini = false
    ) =>
        helper.PartialAsync("Partials/SlotCardPartial", slot, viewData.WithLang(language).WithTime(timeZone)
                .WithKeyValue("User", user)
                .WithKeyValue("CallbackUrl", callbackUrl)
                .WithKeyValue("ShowLink", showLink)
                .WithKeyValue("IsMini", isMini)
                .WithKeyValue("IsMobile", isMobile));

    public static Task<IHtmlContent> ToHtml<T>(this Photo photo, IHtmlHelper<T> helper, ViewDataDictionary<T> viewData, string language, string timeZone, bool canDelete = false)
        => helper.PartialAsync("Partials/PhotoPartial", photo, viewData.WithLang(language).WithTime(timeZone).CanDelete(canDelete));
}