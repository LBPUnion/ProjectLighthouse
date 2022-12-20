using LBPUnion.ProjectLighthouse.Administration;

namespace LBPUnion.ProjectLighthouse.Configuration.ConfigurationCategories;

public class TwoFactorConfiguration
{
    public bool TwoFactorEnabled { get; set; } = true;
    public bool RequireTwoFactor { get; set; } = true;
    public PermissionLevel RequiredTwoFactorLevel { get; set; } = PermissionLevel.Moderator;
}