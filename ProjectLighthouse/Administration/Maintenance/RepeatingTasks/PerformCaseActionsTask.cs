#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Moderation;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Maintenance;
using LBPUnion.ProjectLighthouse.Types.Moderation.Cases;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance.RepeatingTasks;

public class PerformCaseActionsTask : IRepeatingTask
{
    public string Name => "Perform actions on moderation cases";
    public TimeSpan RepeatInterval => TimeSpan.FromSeconds(10);
    public DateTime LastRan { get; set; }
    public async Task Run(DatabaseContext database)
    {
        foreach (ModerationCaseEntity @case in await database.Cases.Where(c => !c.Processed).ToListAsync())
        {
            UserEntity? user = null;
            SlotEntity? slot = null;

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

                        await database.SendNotification(user.UserId,
                            "Your profile comments have been re-enabled by a moderator.");

                        break;
                    }

                    case CaseType.LevelHide:
                    {
                        slot!.Hidden = false;
                        slot.HiddenReason = "";

                        await database.SendNotification(slot.CreatorId,
                            $"Your level, {slot.Name}, is no longer hidden by a moderator.");

                        break;
                    }
                    case CaseType.LevelDisableComments:
                    {
                        slot!.CommentsEnabled = true;

                        await database.SendNotification(slot.CreatorId,
                            $"The comments on your level, {slot.Name}, have been re-enabled by a moderator.");

                        break;
                    }
                    case CaseType.LevelLock:
                    {
                        slot!.InitiallyLocked = false;
                        slot.LockedByModerator = false;
                        slot.LockedReason = "";

                        await database.SendNotification(slot.CreatorId,
                            $"Your level, {slot.Name}, is no longer locked by a moderator.");

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

                        await database.SendNotification(user.UserId,
                            "Your profile comments have been disabled by a moderator.");

                        break;
                    }

                    case CaseType.LevelHide:
                    {
                        slot!.Hidden = true;
                        slot.HiddenReason = @case.Reason;

                        await database.SendNotification(slot.CreatorId,
                            $"Your level, {slot.Name}, has been hidden by a moderator.");

                        break;
                    }
                    case CaseType.LevelDisableComments:
                    {
                        slot!.CommentsEnabled = false;

                        await database.SendNotification(slot.CreatorId,
                            $"The comments on your level, {slot.Name}, have been disabled by a moderator.");

                        break;
                    }
                    case CaseType.LevelLock:
                    {
                        slot!.InitiallyLocked = true;
                        slot.LockedByModerator = true;
                        slot.LockedReason = @case.Reason;

                        await database.SendNotification(slot.CreatorId,
                            $"Your level, {slot.Name}, has been locked by a moderator.");

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