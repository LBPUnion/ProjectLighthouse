using System;
using LBPUnion.ProjectLighthouse.Administration.Maintenance.RepeatingTasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Tests.Helpers;
using LBPUnion.ProjectLighthouse.Types.Entities.Moderation;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LBPUnion.ProjectLighthouse.Tests.Unit;

[Trait("Category", "Unit")]
public class ModerationTests
{
    [Fact]
    public async void DismissExpiredCases_ShouldDismissExpiredCase()
    {
        await using DatabaseContext database = await MockHelper.GetTestDatabase();
        
        ModerationCaseEntity @case = new()
        {
            CaseId = 1,
            ExpiresAt = DateTime.UnixEpoch,
            CreatorUsername = "unitTestUser",
        };
        
        database.Cases.Add(@case);
        
        await database.SaveChangesAsync();
        
        DismissExpiredCasesTask task = new();
        await task.Run(database);
        
        Assert.True(await database.Cases.AnyAsync(c => c.CaseId == 1 && c.DismissedAt != null));
    }

    [Fact]
    public async void DismissExpiredCases_ShouldNotDismissActiveCase()
    {
        await using DatabaseContext database = await MockHelper.GetTestDatabase();

        ModerationCaseEntity @case = new()
        {
            CaseId = 2,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatorUsername = "unitTestUser",
        };

        database.Cases.Add(@case);

        await database.SaveChangesAsync();

        DismissExpiredCasesTask task = new();
        await task.Run(database);

        Assert.True(await database.Cases.AnyAsync(c => c.CaseId == 2 && c.DismissedAt == null));
    }
}