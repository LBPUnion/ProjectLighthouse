using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Services;
using LBPUnion.ProjectLighthouse.Types.Maintenance;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace LBPUnion.ProjectLighthouse.Tests.Unit;

[Trait("Category", "Unit")]
public class RepeatingTaskTests
{
    private class TestTask : IRepeatingTask
    {
        public string Name { get; init; } = "";
        public TimeSpan RepeatInterval => TimeSpan.FromSeconds(5);
        public DateTime LastRan { get; set; }
        public Task Run(DatabaseContext database) => Task.CompletedTask;
    }

    [Fact]
    public void GetNextTask_ShouldReturnNull_WhenTaskListEmpty()
    {
        List<IRepeatingTask> tasks = new();
        IServiceProvider provider = new DefaultServiceProviderFactory().CreateServiceProvider(new ServiceCollection());
        RepeatingTaskService service = new(provider, tasks);

        bool gotTask = service.TryGetNextTask(out IRepeatingTask? outTask);
        Assert.False(gotTask);
        Assert.Null(outTask);
    }

    [Fact]
    public void GetNextTask_ShouldReturnTask_WhenTaskListContainsOne()
    {
        List<IRepeatingTask> tasks = new()
        {
            new TestTask(),
        };
        IServiceProvider provider = new DefaultServiceProviderFactory().CreateServiceProvider(new ServiceCollection());
        RepeatingTaskService service = new(provider, tasks);

        bool gotTask = service.TryGetNextTask(out IRepeatingTask? outTask);
        Assert.True(gotTask);
        Assert.NotNull(outTask);
    }

    [Fact]
    public void GetNextTask_ShouldReturnShortestTask_WhenTaskListContainsMultiple()
    {
        List<IRepeatingTask> tasks = new()
        {
            new TestTask
            {
                Name = "Task 1",
                LastRan = DateTime.UtcNow,
            },
            new TestTask
            {
                Name = "Task 2",
                LastRan = DateTime.UtcNow.AddMinutes(1),
            },
            new TestTask
            {
                Name = "Task 3",
                LastRan = DateTime.UtcNow.AddMinutes(-1),
            },
        };
        IServiceProvider provider = new DefaultServiceProviderFactory().CreateServiceProvider(new ServiceCollection());
        RepeatingTaskService service = new(provider, tasks);

        bool gotTask = service.TryGetNextTask(out IRepeatingTask? outTask);
        Assert.True(gotTask);
        Assert.NotNull(outTask);
        Assert.Equal("Task 3", outTask.Name);
    }

    [Fact]
    public async Task BackgroundService_ShouldExecuteTask_AndUpdateTime()
    {
        Mock<IRepeatingTask> taskMock = new();
        taskMock.Setup(t => t.Run(It.IsAny<DatabaseContext>())).Returns(Task.CompletedTask);
        taskMock.Setup(t => t.RepeatInterval).Returns(TimeSpan.FromSeconds(10));
        taskMock.SetupProperty(t => t.LastRan);
        List<IRepeatingTask> tasks = new()
        {
            taskMock.Object,
        };
        ServiceCollection serviceCollection = new();
        serviceCollection.AddScoped(_ => new Mock<DatabaseContext>().Object);
        IServiceProvider provider = new DefaultServiceProviderFactory().CreateServiceProvider(serviceCollection);

        RepeatingTaskService service = new(provider, tasks);

        CancellationTokenSource tokenSource = new();

        await service.StartAsync(tokenSource.Token);

        tokenSource.CancelAfter(1);
        Assert.NotNull(service.ExecuteTask);
        await service.ExecuteTask;

        taskMock.Verify(x => x.Run(It.IsAny<DatabaseContext>()), Times.Once);
        taskMock.VerifySet(x => x.LastRan = It.IsAny<DateTime>(), Times.Once());
    }
}