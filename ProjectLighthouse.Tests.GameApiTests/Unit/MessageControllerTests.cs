using LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests.Unit;

[Trait("Category", "Unit")]
public class MessageControllerTests
{

    [Fact]
    public async void ShouldSendAnnouncement()
    {
        // Mock<>
        MessageController messageController = new(null);
        
        IActionResult result = await messageController.Announce();
    }
    
}