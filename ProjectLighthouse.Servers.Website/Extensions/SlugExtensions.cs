using System.Text.RegularExpressions;
using System.Web;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Extensions;

public static partial class SlugExtensions
{
    [GeneratedRegex("[^a-zA-Z0-9 ]")]
    private static partial Regex ValidSlugCharactersRegex();

    [GeneratedRegex(@"[\s]{2,}")]
    private static partial Regex WhitespaceRegex();

    /// <summary>
    /// Generates a URL slug that only contains alphanumeric characters
    /// with spaces replaced with dashes
    /// </summary>
    /// <param name="slot">The slot to generate the slug for</param>
    /// <returns>A string containing the url slug for this slot</returns>
    public static string GenerateSlug(this SlotEntity slot) =>
        slot.Name.Length == 0
            ? "unnamed-level"
            : WhitespaceRegex().Replace(ValidSlugCharactersRegex().Replace(HttpUtility.HtmlDecode(slot.Name), ""), " ").Trim().Replace(" ", "-").ToLower();

    /// <summary>
    /// Generates a URL slug for the given user
    /// </summary>
    /// <param name="user">The user to generate the slug for</param>
    /// <returns>A string containing the url slug for this user</returns>
    public static string GenerateSlug(this UserEntity user) => user.Username.ToLower();
}