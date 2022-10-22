using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;

#nullable enable

public class ACTActionCollection {
    [Key]
    public int ActionId { get; set; }

    public int ActorId { get; set; }
    
    [ForeignKey(nameof(ActorId))]
    public User? Actor { get; set; }
    
    public int ObjectId { get; set; }
    
    public int ActionType { get; set; }

    public long ActionTimestamp { get; set; }

    public int? Interaction { get; set; }
    public int? Interaction2 { get; set; }
}