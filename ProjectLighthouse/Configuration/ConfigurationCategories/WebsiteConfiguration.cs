namespace LBPUnion.ProjectLighthouse.Configuration.ConfigurationCategories;

public class WebsiteConfiguration
{
    public string MissingIconHash { get; set; } = "";

    public bool ConvertAssetsOnStartup { get; set; } = true;

    /// <summary>
    ///     Displays the Lighthouse Pride logo during the month of June.
    /// </summary>
    public bool PrideEventEnabled { get; set; } = true;

    public bool ShowOnlinePlayers { get; set; } = true;
}