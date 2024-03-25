using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Tests.Helpers;
using LBPUnion.ProjectLighthouse.Types.Activity;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests.Unit.Activity;

[Trait("Category", "Unit")]
public class ActivityInterceptorTests
{
    private static async Task<DatabaseContext> GetTestDatabase(IMock<IEntityEventHandler> eventHandlerMock)
    {
        DbContextOptionsBuilder<DatabaseContext> optionsBuilder = await MockHelper.GetInMemoryDbOptions();
        
        optionsBuilder.AddInterceptors(new ActivityInterceptor(eventHandlerMock.Object));
        DatabaseContext database = new(optionsBuilder.Options);
        await database.Database.EnsureCreatedAsync();
        return database;
    }

    [Fact]
    public async Task SaveChangesWithNewEntity_ShouldCallEntityInserted()
    {
        Mock<IEntityEventHandler> eventHandlerMock = new();
        DatabaseContext database = await GetTestDatabase(eventHandlerMock);

        database.Users.Add(new UserEntity
        {
            UserId = 1,
            Username = "test",
        });
        await database.SaveChangesAsync();

        eventHandlerMock.Verify(x => x.OnEntityInserted(It.IsAny<DatabaseContext>(), It.Is<object>(user => user is UserEntity)), Times.Once);
    }

    [Fact]
    public async Task SaveChangesWithModifiedEntity_ShouldCallEntityChanged()
    {
        Mock<IEntityEventHandler> eventHandlerMock = new();
        DatabaseContext database = await GetTestDatabase(eventHandlerMock);

        UserEntity user = new()
        {
            Username = "test",
        };

        database.Users.Add(user);
        await database.SaveChangesAsync();

        user.Username = "test2";
        await database.SaveChangesAsync();

        eventHandlerMock.Verify(x => x.OnEntityChanged(It.IsAny<DatabaseContext>(),
                It.Is<object>(u => u is UserEntity && ((UserEntity)u).Username == "test"),
                It.Is<object>(u => u is UserEntity && ((UserEntity)u).Username == "test2")),
            Times.Once);
    }

    [Fact]
    public async Task SaveChangesWithModifiedEntity_ShouldCallEntityDeleted()
    {
        Mock<IEntityEventHandler> eventHandlerMock = new();
        DatabaseContext database = await GetTestDatabase(eventHandlerMock);

        UserEntity user = new()
        {
            Username = "test",
        };

        database.Users.Add(user);
        await database.SaveChangesAsync();

        database.Users.Remove(user);
        await database.SaveChangesAsync();

        eventHandlerMock.Verify(x => x.OnEntityDeleted(It.IsAny<DatabaseContext>(), It.Is<object>(u => u is UserEntity)), Times.Once);
    }
}