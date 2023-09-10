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
    public async void CanDismissExpiredCases()
    {
        await using DatabaseContext database = await IntegrationHelper.GetIntegrationDatabase();
        
        ModerationCaseEntity @case = new()
        {
            CaseId = 1,
            ExpiresAt = DateTime.Now - TimeSpan.FromHours(1),
            CreatorUsername = "unitTestUser",
        };
        
        database.Cases.Add(@case);
        
        await database.SaveChangesAsync();
        
        DismissExpiredCasesTask task = new();
        await task.Run(database);
        
        Assert.False(await database.Cases.AnyAsync(c => c.CaseId == 1));
    }

    [Fact]
    public async void DoNotDismissActiveCases()
    {
        await using DatabaseContext database = await IntegrationHelper.GetIntegrationDatabase();

        ModerationCaseEntity @case = new()
        {
            CaseId = 2,
            ExpiresAt = DateTime.Now + TimeSpan.FromHours(1),
            CreatorUsername = "unitTestUser",
        };

        database.Cases.Add(@case);

        await database.SaveChangesAsync();

        DismissExpiredCasesTask task = new();
        await task.Run(database);

        Assert.True(await database.Cases.AnyAsync(c => c.CaseId == 2));
    }
}