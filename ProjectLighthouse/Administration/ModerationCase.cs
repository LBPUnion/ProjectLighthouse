using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Administration;
#nullable enable

public class ModerationCase
{
    [Key]
    public int CaseId { get; set; }
    
    public CaseType CaseType { get; set; }
    
    public string CaseDescription { get; set; }
    
    public DateTime CaseCreated { get; set; }
    
    public DateTime? CaseExpires { get; set; }
    public bool Expired => this.CaseExpires != null && this.CaseExpires < DateTime.Now;
    
    public int CaseCreatorId { get; set; }
    
    [ForeignKey(nameof(CaseCreatorId))]
    public User? CaseCreator { get; set; }
    
    public int AffectedId { get; set; }

    #region Get affected id result
    public Task<User> GetUserAsync(Database database)
    {
        Debug.Assert(CaseType.AffectsUser());
        return database.Users.FirstOrDefaultAsync(u => u.UserId == this.AffectedId)!;
    }
    
    public Task<Slot> GetSlotAsync(Database database)
    {
        Debug.Assert(CaseType.AffectsLevel());
        return database.Slots.FirstOrDefaultAsync(u => u.SlotId == this.AffectedId)!;
    }
    #endregion

    public static ModerationCase NewTeamPickCase(int caseCreator, int slotId, bool added) => new()
    {
        CaseType = added ? CaseType.LevelTeamPickAdded : CaseType.LevelTeamPickRemoved,
        CaseDescription = "",
        CaseCreatorId = caseCreator,
        CaseCreated = DateTime.Now,
        AffectedId = slotId,
    };
}