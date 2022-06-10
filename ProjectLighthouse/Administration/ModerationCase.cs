using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;

namespace LBPUnion.ProjectLighthouse.Administration;
#nullable enable

public class ModerationCase
{
    [Key]
    public int CaseId { get; set; }
    
    public CaseType CaseType { get; set; }
    
    public string CaseDescription { get; set; }
    
    public DateTime CaseCreated { get; set; }
    
    public int CaseCreatorId { get; set; }
    
    [ForeignKey(nameof(CaseCreatorId))]
    public User? CaseCreator { get; set; }
}