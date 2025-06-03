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
        foreach (string userAgent in validUserAgents)
        {
            result = PatchworkHelper.IsValidPatchworkUserAgent(userAgent);
            Assert.True(result, $"Valid user agent: \"{userAgent}\" was evaluated as {result}.");
        }
        foreach (string userAgent in invalidUserAgents)
        {
            result = PatchworkHelper.IsValidPatchworkUserAgent(userAgent);
            Assert.False(result, $"Invalid user agent: \"{userAgent}\" was evaluated as {result}.");
        }
    }
}