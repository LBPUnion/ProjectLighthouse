#nullable enable
using System;
using System.Collections.Generic;
using LBPUnion.ProjectLighthouse.Localization;
using LBPUnion.ProjectLighthouse.Localization.StringLists;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;

public class BaseLayout : PageModel
{
    public readonly Database Database;

    public readonly List<PageNavigationItem> NavigationItems = new();

    public readonly List<PageNavigationItem> NavigationItemsRight = new();
    public string Description = string.Empty;

    public bool IsMobile;

    public bool ShowTitleInPage = true;

    public string Title = string.Empty;

    private User? user;
    public BaseLayout(Database database)
    {
        this.Database = database;

        this.NavigationItems.Add(new PageNavigationItem(BaseLayoutStrings.HeaderHome, "/", "home"));
        this.NavigationItems.Add(new PageNavigationItem(BaseLayoutStrings.HeaderUsers, "/users/0", "user friends"));
        this.NavigationItems.Add(new PageNavigationItem(BaseLayoutStrings.HeaderPhotos, "/photos/0", "camera"));
        this.NavigationItems.Add(new PageNavigationItem(BaseLayoutStrings.HeaderSlots, "/slots/0", "certificate"));
    }

    public new User? User {
        get {
            if (this.user != null) return this.user;

            return this.user = this.Database.UserFromWebRequest(this.Request);
        }
        set => this.user = value;
    }

    public string Translate(TranslatableString translatableString)
    {
        string lang;
        IRequestCultureFeature? requestCulture = Request.HttpContext.Features.Get<IRequestCultureFeature>();

        if (requestCulture == null) lang = LocalizationManager.DefaultLang;
        else
        {
            lang = requestCulture.RequestCulture.UICulture.Name;
        }

        Console.WriteLine(lang);

        return translatableString.Translate(lang);
    }
}