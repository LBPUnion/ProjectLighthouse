#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.Logging;
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
                if (user == null)
                {
                    Logger.Error($"Target user for case {@case.CaseId} is null (userId={@case.AffectedId})", LogArea.Maintenance);
                    @case.Processed = true;
                    continue;
                }
            }
            else if (@case.Type.AffectsLevel())
            {
                slot = await @case.GetSlotAsync(database);
                if (slot == null)
                {
                    Logger.Error($"Target slot for case {@case.CaseId} is null (slotId={@case.AffectedId})", LogArea.Maintenance);
                    // Just mark as processed, this needs to be handled better in the future
                    @case.Processed = true;
                    continue;
                }
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
                    case CaseType.UserDisableComments:
                    {
                        user!.CommentsEnabled = true;
                        break;
                    }
                    
                    case CaseType.LevelHide:
                    {
                        slot!.Hidden = false;
                        slot.HiddenReason = "";
                        break;
                    }
                    case CaseType.LevelDisableComments:
                    {
                        slot!.CommentsEnabled = true;
                        break;
                    }
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
                        user.BannedReason = @case.Reason;
                        
                        database.GameTokens.RemoveRange(database.GameTokens.Where(t => t.UserId == user.UserId));
                        database.WebTokens.RemoveRange(database.WebTokens.Where(t => t.UserId == user.UserId));
                        break;
                    }
                    case CaseType.UserDisableComments:
                    {
                        user!.CommentsEnabled = false;
                        break;
                    }

                    case CaseType.LevelHide:
                    {
                        slot!.Hidden = true;
                        slot.HiddenReason = @case.Reason;
                        
                        break;
                    }
                    case CaseType.LevelDisableComments:
                    {
                        slot!.CommentsEnabled = false;
                        break;
                    }
                    default: throw new ArgumentOutOfRangeException();
                }
            }

            @case.Processed = true;
        }

        await database.SaveChangesAsync();
    }
}