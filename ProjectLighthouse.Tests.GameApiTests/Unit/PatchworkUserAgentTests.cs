using LBPUnion.ProjectLighthouse.Servers.GameServer.Helpers;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests.Unit;

[Trait("Category", "Unit")]
public class PatchworkUserAgentTests
{
    [Fact]
    public void CanValidatePatchworkUserAgents()
    {
        string[] validUserAgents = {
            "PatchworkLBP1 1.0",
            "PatchworkLBP2 2.0",
            "PatchworkLBP3 3.0",
            "PatchworkLBPV 4.0",
            "PatchworkLBP1 1.5",
        };

        string[] invalidUserAgents = {
            // Matching
            "patchworklbp1 1.0", // Case sensitive
            "ptchwrklbp1 1.0", // Misspelled
            "PatchworkLBP1 1", // Missing major/minor
            "PatchworkLBP1 1.000001", // Major/minor too long

            // Data
            "PatchworkLBP1 0.5", // Version number too low
            "PatchworkLBP1 A.0" // Int cannot be parsed
        };

        bool result;
        for (int i = 0; i < validUserAgents.Length; i++)
        {
            result = PatchworkHelper.IsValidPatchworkUserAgent(validUserAgents[i]);
            Assert.True(result, $"Valid user agent: \"{validUserAgents[i]}\" was evaluated as {result}.");
        }
        for (int i = 0; i < invalidUserAgents.Length; i++)
        {
            result = PatchworkHelper.IsValidPatchworkUserAgent(invalidUserAgents[i]);
            Assert.False(result, $"Invalid user agent: \"{invalidUserAgents[i]}\" was evaluated as {result}.");
        }
    }
}