using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Profile;

[PrimaryKey(nameof(UserId), nameof(PinSet), nameof(ProgressType))]
public class UserPinProgressEntity
{
    public int UserId { get; set; }

    public UserEntity User { get; set; } = null!;

    public PinSet PinSet { get; set; }

    public uint ProgressType { get; set; }

    public double Value { get; set; }
}
