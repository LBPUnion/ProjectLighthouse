using System;

namespace LBPUnion.ProjectLighthouse.Types.Settings.ConfigurationCategories;

public class AuthenticationConfiguration
{
    [Obsolete("Obsolete. This feature has been removed.", true)]
    public bool BlockDeniedUsers { get; set; }

    public bool RegistrationEnabled { get; set; } = true;
    public bool UseExternalAuth { get; set; }
}