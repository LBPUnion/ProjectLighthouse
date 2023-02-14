#nullable enable

using LBPUnion.ProjectLighthouse.Localization;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Types;

public class PageNavigationItem
{
    public PageNavigationItem(TranslatableString name, string url, string? icon = null, string? customColor = null)
    {
        this.Name = name;
        this.Url = url;
        this.Icon = icon;
        this.CustomColor = customColor;
    }

    public TranslatableString Name { get; set; }
    public string Url { get; set; }
    public string? Icon { get; set; }
    public string? CustomColor { get; set; }
}