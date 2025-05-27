namespace LBPUnion.ProjectLighthouse.Configuration.ConfigurationCategories;

public class AuthenticationConfiguration
{
    public bool RegistrationEnabled { get; set; } = true;
    public bool AutomaticAccountCreation { get; set; } = true;
    public bool VerifyTickets { get; set; } = true;

    public bool AllowRPCNSignup { get; set; } = true;

    public bool AllowPSNSignup { get; set; } = true;

    // Require use of Zaprit's "Patchwork" prx plugin's user agent when connecting to the server
    // Major and minor version minimums can be left alone if patchwork is not required
    public bool RequirePatchworkUserAgent { get; set; } = false;
    public int PatchworkMajorVersionMinimum { get; set; } = 1;
    public int PatchworkMinorVersionMinimum { get; set; } = 0;
    
}