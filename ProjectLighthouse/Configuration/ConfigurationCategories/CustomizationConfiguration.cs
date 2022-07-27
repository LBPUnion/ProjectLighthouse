namespace LBPUnion.ProjectLighthouse.Configuration.ConfigurationCategories;

public class CustomizationConfiguration
{
    public string ServerName { get; set; } = "Project Lighthouse";
    public string EnvironmentName { get; set; } = "lighthouse";
    public bool UseNumericRevisionNumber { get; set; } = false;
}