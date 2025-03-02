#nullable enable
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace LBPUnion.ProjectLighthouse.Configuration;

public class EmailEnforcementConfiguration : ConfigurationBase<EmailEnforcementConfiguration>
{
    public override int ConfigVersion { get; set; } = 1;

    public override string ConfigName { get; set; } = "enforce-email.yml";

    public override bool NeedsConfiguration { get; set; } = false;

    public bool EnableEmailEnforcement { get; set; } = false;
    public bool EnableEmailBlacklist { get; set; } = false;

    // No blacklist by default, add path to blacklist
    public string BlacklistFilePath { get; set; } = "";

    // Endpoints to be blocked
    // This is kind of a random list so some may need to be added or removed
    public HashSet<string> BlockedEndpoints { get; set; } = new()
    { 
        // Comments
        "rateUserComment",
        "rateComment",
        "comments",
        "userComments",
        "postUserComment",
        "postComment",
        "deleteUserComment",
        "deleteComment", 

        // Slots
        "showModerated", 
        "startPublish",
        "slots", 
        "s", 
        "tags",
        "tag", 
        "searches",
        "genres",
        "publish",
        "unpublish",

        // Misc Resources
        "upload",
        "r",
        
        // Photos
        "uploadPhoto",
        "photos",
        "deletePhoto",

        // Gameplay
        "match",
        "play",
        "enterLevel", 
        "playlists",

        // Users
        "user",
        "users",
        "updateUser",
        "update_my_pins",
    };

    public override ConfigurationBase<EmailEnforcementConfiguration> Deserialize
        (IDeserializer deserializer, string text) =>
        deserializer.Deserialize<EmailEnforcementConfiguration>(text);
}