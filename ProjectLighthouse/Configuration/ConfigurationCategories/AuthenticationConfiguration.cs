using System;

namespace LBPUnion.ProjectLighthouse.Configuration.ConfigurationCategories;

public class AuthenticationConfiguration
{
    public bool RegistrationEnabled { get; set; } = true;
    public bool PrivateRegistration { get; set; } = false;
    public bool UseExternalAuth { get; set; }
}