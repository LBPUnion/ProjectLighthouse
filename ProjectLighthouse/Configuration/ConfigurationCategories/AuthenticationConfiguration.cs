namespace LBPUnion.ProjectLighthouse.Configuration.ConfigurationCategories;

public class AuthenticationConfiguration
{
    public bool RegistrationEnabled { get; set; } = true;
    public bool AutomaticAccountCreation { get; set; } = true;
    public bool VerifyTickets { get; set; } = true;
}