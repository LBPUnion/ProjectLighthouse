#nullable enable
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;

namespace LBPUnion.ProjectLighthouse.Configuration;

public class EnforceEmailConfiguration : ConfigurationBase<EnforceEmailConfiguration>
{
    public override int ConfigVersion { get; set; } = 2;

    public override string ConfigName { get; set; } = "enforce-email.yml";

    public override bool NeedsConfiguration { get; set; } = false;

    public bool EnableEmailEnforcement { get; set; } = false;
    public bool EnableEmailBlacklist { get; set; } = false;

    // No blacklist by default, add path to blacklist
    public string BlacklistFilePath { get; set; } = "";

    public string DomainWhitelist { get; set; } = "";
                                                 
    // TODO: Finalize what to do with these
    // Keep warnings here for debug until pull
    public string EmailEnforcementMessageMain { get; set; } = 
        "This lighthouse instance has email enforcement enabled. " +
        "If you haven't already, you will need to set and verify " +
        "an email address to use most features.\n";

    public string EmailEnforcementMessageNoEmail { get; set; } = 
        "You do not have an email set on your account. You can set " +
        "an email by opening the text chat and typing \"<em>/setemail " +
        "[youremail@example.com]</em>\" (do not include the brackets.)\n\n";

    public string EmailEnforcementMessageVerify { get; set; } = 
        "You have set an email address on your account, but you have not " +
        "verified it. Make sure to check your inbox for a verification email. " +
        "If you have not received an email, please contact an instance " +
        "administrator for further assistance.\n\n";

    public override ConfigurationBase<EnforceEmailConfiguration> Deserialize
        (IDeserializer deserializer, string text) =>
        deserializer.Deserialize<EnforceEmailConfiguration>(text);
}