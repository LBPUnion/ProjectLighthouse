#nullable enable
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace LBPUnion.ProjectLighthouse.Configuration;

public class CategoryConfiguration : ConfigurationBase<CategoryConfiguration>
{
    // HEY, YOU!
    // THIS VALUE MUST BE INCREMENTED FOR EVERY CONFIG CHANGE!
    //
    // This is so Lighthouse can properly identify outdated configurations and update them with newer settings accordingly.
    // If you are modifying anything here, this value MUST be incremented.
    // Thanks for listening~
    public override int ConfigVersion { get; set; } = 2;
    public override string ConfigName { get; set; } = "CategoryConfig.yml";
    public override bool NeedsConfiguration { get; set; } = false;

    public List<string> Categories { get; set; } = new()
    {
        "recently_played",
        "recommended",
        "team_picks",
        "most_hearted",
        "newest",
        "busiest",
        "most_played",
        "my_playlists",
        "queue",
        "hearted_levels",
        "highest_rated",
        "lucky_dip",
    };

    public RecommendedCategoryConfig Recommended { get; set; } = new();
    public RecentlyPlayedConfig RecentlyPlayed { get; set; } = new();

    public override ConfigurationBase<CategoryConfiguration> Deserialize(IDeserializer deserializer, string text) =>
        deserializer.Deserialize<CategoryConfiguration>(text);
}

public class RecommendedCategoryConfig
{
    public int MaxNeighbors { get; set; } = 250;
    public int MinimumNeighborOverlap { get; set; } = 2;
    public int MaxCandidatePool { get; set; } = 1000;
}

public class RecentlyPlayedConfig
{
    public int MaxLevels { get; set; } = 30;
}
