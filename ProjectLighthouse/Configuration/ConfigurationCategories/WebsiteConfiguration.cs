namespace LBPUnion.ProjectLighthouse.Configuration.ConfigurationCategories;

public class WebsiteConfiguration
{
    public string MissingIconHash { get; set; } = "";

    public bool ConvertAssetsOnStartup { get; set; } = true;

    /*
     * Decides whether or not to display the Lighthouse Pride logo
     * during the month of June if enabled.
     */
    public bool PrideEventEnabled { get; set; } = true;
}