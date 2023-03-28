#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Level;

public class PlaylistEntity
{
    [Key]
    public int PlaylistId { get; set; }

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    public int CreatorId { get; set; }

    [ForeignKey(nameof(CreatorId))]
    public UserEntity? Creator { get; set; }

    public string SlotCollection { get; set; } = "";

    [NotMapped]
    public int[] SlotIds
    {
        get => this.SlotCollection.Split(",").Where(x => int.TryParse(x, out _)).Select(int.Parse).ToArray();
        set => this.SlotCollection = string.Join(",", value);
    }

}