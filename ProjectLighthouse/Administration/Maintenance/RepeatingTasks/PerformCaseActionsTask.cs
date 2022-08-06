#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance.RepeatingTasks;

public class PerformCaseActionsTask : IRepeatingTask
{
    public string Name => "Perform actions on moderation cases";
    public TimeSpan RepeatInterval => TimeSpan.FromSeconds(10);
    public DateTime LastRan { get; set; }
    public async Task Run(Database database)
    {
        foreach (ModerationCase @case in await database.Cases.Where(c => !c.Processed).ToListAsync())
        {
            User? user = null;
            Slot? slot = null;

            if (@case.Type.AffectsUser())
            {
                user = await @case.GetUserAsync(database);
            }
            else if(@case.Type.AffectsLevel())
            {
                slot = await @case.GetSlotAsync(database);
            }
            
            if (@case.Expired || @case.Dismissed)
            {
                switch (@case.Type)
                {
                    case CaseType.UserBan:
                    case CaseType.UserRestriction:
                    case CaseType.UserSilence:
                    {
                        user!.PermissionLevel = PermissionLevel.Default;
                        break;
                    };
                    case CaseType.UserCommentsDisabled: break;
                    case CaseType.LevelLock: break;
                    case CaseType.LevelCommentsDisabled: break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                switch (@case.Type)
                {
                    case CaseType.UserSilence:
                    {
                        user!.PermissionLevel = PermissionLevel.Silenced;
                        break;
                    }
                    case CaseType.UserRestriction:
                    {
                        user!.PermissionLevel = PermissionLevel.Restricted;
                        break;
                    }
                    case CaseType.UserBan:
                    {
                        user!.PermissionLevel = PermissionLevel.Banned;
                        break;
                    }
                    case CaseType.UserCommentsDisabled: break;
                    case CaseType.LevelLock: break;
                    case CaseType.LevelCommentsDisabled: break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }

            @case.Processed = true;
        }

        await database.SaveChangesAsync();
    }
}