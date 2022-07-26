using System;

namespace LBPUnion.ProjectLighthouse.Configuration.ConfigurationCategories;

public class AuthenticationConfiguration
{
    [Obsolete("Obsolete. This feature has been removed.", true)]
    public bool BlockDeniedUsers { get; set; }

    public bool RegistrationEnabled { get; set; } = true;
    public bool PrivateRegistration { get; set; } = false;
    public bool UseExternalAuth { get; set; }
}