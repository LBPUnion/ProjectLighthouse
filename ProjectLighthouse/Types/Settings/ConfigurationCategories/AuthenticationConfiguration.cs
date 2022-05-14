namespace LBPUnion.ProjectLighthouse.Types.Settings.ConfigurationCategories;

public class AuthenticationConfiguration
{
    public bool BlockDeniedUsers { get; set; } = true;
    public bool RegistrationEnabled { get; set; } = true;
    public bool UseExternalAuth { get; set; }
}