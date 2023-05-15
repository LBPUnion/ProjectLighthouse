using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers;
using LBPUnion.ProjectLighthouse.Tests.Helpers;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Levels;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests.Unit.Controllers;

[Trait("Category", "Unit")]
public class CommentControllerTests
{
    [Fact]
    public async Task PostComment_ShouldPostProfileComment_WhenBodyIsValid()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        CommentController commentController = new(dbMock);
        commentController.SetupTestController("<comment><message>test</message></comment>");

        const string expectedCommentMessage = "test";

        IActionResult result = await commentController.PostComment("unittest", null, 0);

        Assert.IsType<OkResult>(result);
        CommentEntity? comment = dbMock.Comments.FirstOrDefault();
        Assert.NotNull(comment);
        Assert.Equal(expectedCommentMessage, comment.Message);
    }

    [Fact]
    public async Task PostComment_ShouldCensorComment_WhenFilterEnabled()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        CommentController commentController = new(dbMock);
        commentController.SetupTestController("<comment><message>zamn</message></comment>");

        CensorConfiguration.Instance.FilteredWordList = new List<string>
        {
            "zamn",
        };
        CensorConfiguration.Instance.UserInputFilterMode = FilterMode.Asterisks;
        const string expectedCommentMessage = "****";

        IActionResult result = await commentController.PostComment("unittest", null, 0);


        Assert.IsType<OkResult>(result);
        CommentEntity? comment = dbMock.Comments.FirstOrDefault();
        Assert.NotNull(comment);
        Assert.Equal(expectedCommentMessage, comment.Message);
    }

    [Fact]
    public async Task PostComment_ShouldCensorComment_WhenFilterDisabled()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        CommentController commentController = new(dbMock);
        commentController.SetupTestController("<comment><message>zamn</message></comment>");

        CensorConfiguration.Instance.FilteredWordList = new List<string>
        {
            "zamn",
        };
        CensorConfiguration.Instance.UserInputFilterMode = FilterMode.None;

        IActionResult result = await commentController.PostComment("unittest", null, 0);

        const string expectedCommentMessage = "zamn";

        Assert.IsType<OkResult>(result);
        CommentEntity? comment = dbMock.Comments.FirstOrDefault();
        Assert.NotNull(comment);
        Assert.Equal(expectedCommentMessage, comment.Message);
    }

    [Fact]
    public async Task PostComment_ShouldPostUserLevelComment_WhenBodyIsValid()
    {
        List<SlotEntity> slots = new()
        {
            new SlotEntity
            {
                SlotId = 1,
                CreatorId = 1,
                Type = SlotType.User,
            },
        };
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase(new[]{slots,});

        CommentController commentController = new(dbMock);
        commentController.SetupTestController("<comment><message>test</message></comment>");

        const string expectedCommentMessage = "test";

        IActionResult result = await commentController.PostComment(null, "user", 1);

        Assert.IsType<OkResult>(result);
        CommentEntity? comment = dbMock.Comments.FirstOrDefault();
        Assert.NotNull(comment);
        Assert.Equal(expectedCommentMessage, comment.Message);
    }

    [Fact]
    public async Task PostComment_ShouldPostDeveloperLevelComment_WhenBodyIsValid()
    {
        List<SlotEntity> slots = new()
        {
            new SlotEntity
            {
                SlotId = 1,
                InternalSlotId = 12345,
                CreatorId = 1,
                Type = SlotType.Developer,
            },
        };
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase(new[] { slots, });

        CommentController commentController = new(dbMock);
        commentController.SetupTestController("<comment><message>test</message></comment>");

        const string expectedCommentMessage = "test";

        IActionResult result = await commentController.PostComment(null, "developer", 12345);

        Assert.IsType<OkResult>(result);
        CommentEntity? comment = dbMock.Comments.FirstOrDefault();
        Assert.NotNull(comment);
        Assert.Equal(expectedCommentMessage, comment.Message);
    }

    [Fact]
    public async Task PostComment_ShouldNotPostProfileComment_WhenTargetProfileInvalid()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        CommentController commentController = new(dbMock);
        commentController.SetupTestController("<comment><message>test</message></comment>");

        IActionResult result = await commentController.PostComment("unittest2", null, 0);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task PostComment_ShouldNotPostUserLevelComment_WhenLevelInvalid()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        CommentController commentController = new(dbMock);
        commentController.SetupTestController("<comment><message>test</message></comment>");

        IActionResult result = await commentController.PostComment(null, "user", 1);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task PostComment_ShouldNotPostComment_WhenBodyIsEmpty()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        CommentController commentController = new(dbMock);
        commentController.SetupTestController("");

        IActionResult result = await commentController.PostComment("unittest", null, 0);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task PostComment_ShouldNotPostComment_WhenBodyIsInvalid()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        CommentController commentController = new(dbMock);
        commentController.SetupTestController("<comment></comment>");

        IActionResult result = await commentController.PostComment("unittest", null, 0);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task PostComment_ShouldFail_WhenSlotTypeIsInvalid()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        CommentController commentController = new(dbMock);
        commentController.SetupTestController("<comment><message>test</message></comment>");

        IActionResult result = await commentController.PostComment(null, "banana", 0);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task PostComment_ShouldFail_WhenAllArgumentsAreEmpty()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        CommentController commentController = new(dbMock);
        commentController.SetupTestController("<comment><message>test</message></comment>");

        IActionResult result = await commentController.PostComment(null, null, 0);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task PostComment_ShouldFail_WhenSlotTypeAndUsernameAreProvided()
    {
        await using DatabaseContext dbMock = await MockHelper.GetTestDatabase();

        CommentController commentController = new(dbMock);
        commentController.SetupTestController("<comment><message>test</message></comment>");

        IActionResult result = await commentController.PostComment("unittest", "user", 10);

        Assert.IsType<BadRequestResult>(result);
    }
    
}