using LBPUnion.ProjectLighthouse.Localization;

#nullable enable

namespace LBPUnion.ProjectLighthouse.Types;

public class PageNavigationItem
{
    public PageNavigationItem(TranslatableString name, string url, string? icon = null)
    {
        this.Name = name;
        this.Url = url;
        this.Icon = icon;
    }
    public TranslatableString Name { get; set; }
    public string Url { get; set; }
    public string? Icon { get; set; }
}