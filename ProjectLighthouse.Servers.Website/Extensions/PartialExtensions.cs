using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Extensions;

public static class PartialExtensions
{
    // ReSharper disable once SuggestBaseTypeForParameter
    public static ViewDataDictionary<T> WithLang<T>(this ViewDataDictionary<T> viewData, string language)
    {
        try
        {
            return new(viewData)
            {
                {
                    "Language", language
                },
            };
        }
        catch
        {
            return viewData;
        }
    }

    public static Task<IHtmlContent> ToLink<T>
    (
        this User user,
        IHtmlHelper<T> helper,
        ViewDataDictionary<T> viewData,
        string language,
        bool includeUserStatus = false
    )
    {
        if(viewData.ContainsKey("IncludeStatus"))
            viewData.Remove("IncludeStatus");
        if(includeUserStatus)
            viewData.Add("IncludeStatus", true);

        return helper.PartialAsync("Partials/Links/UserLinkPartial", user, viewData.WithLang(language));
    } 
        

    public static Task<IHtmlContent> ToHtml<T>(this Photo photo, IHtmlHelper<T> helper, ViewDataDictionary<T> viewData, string language)
        => helper.PartialAsync("Partials/PhotoPartial", photo, viewData.WithLang(language));
}