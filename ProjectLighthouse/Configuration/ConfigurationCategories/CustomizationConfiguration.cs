namespace LBPUnion.ProjectLighthouse.Configuration.ConfigurationCategories;

public class CustomizationConfiguration
{
    public string ServerName { get; set; } = "Project Lighthouse";
    public string EnvironmentName { get; set; } = "project-lighthouse";
    public bool UseLessReliableNumericRevisionNumberingSystem { get; set; } = false;
}