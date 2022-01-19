#nullable enable

namespace LBPUnion.ProjectLighthouse.Types;

public class PageNavigationItem
{
    public PageNavigationItem(string name, string url, string? icon = null)
    {
        this.Name = name;
        this.Url = url;
        this.Icon = icon;
    }
    public string Name { get; set; }
    public string Url { get; set; }
    public string? Icon { get; set; }
}