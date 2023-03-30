using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ProjectLighthouse.Tests.GameApiTests.Unit;

public static class MockHelper
{

    public static ControllerContext GetMockControllerContext() =>
        new()
        {
            HttpContext = new DefaultHttpContext(),
        };

    public static void SetupTestGameToken(ControllerBase controller, GameTokenEntity token)
    {
        controller.HttpContext.Items["Token"] = token;
    }
}