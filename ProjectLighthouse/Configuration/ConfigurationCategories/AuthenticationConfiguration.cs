namespace LBPUnion.ProjectLighthouse.Configuration.ConfigurationCategories;

public class AuthenticationConfiguration
{
    public bool RegistrationEnabled { get; set; } = true;
    public bool AutomaticAccountCreation { get; set; } = true;
    public bool VerifyTickets { get; set; } = true;

    public bool AllowRPCNSignup { get; set; } = true;

    public bool AllowPSNSignup { get; set; } = true;
    
}