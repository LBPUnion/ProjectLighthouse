using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Levels;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

[XmlInclude(typeof(UserSlot))]
[XmlInclude(typeof(DeveloperSlot))]
// The C# XML serializer doesn't recognize children of interfaces, only abstract classes. Therefore, this has to be abstract
public abstract class SlotBase : ILbpSerializable
{

    public static SlotBase CreateFromEntity(SlotEntity slot, GameToken token, SerializationMode serializationMode)
    {
        SlotBase retSlot = CreateFromEntity(slot, token);
        if (retSlot is UserSlot userSlot) userSlot.SerializationMode = serializationMode;

        return retSlot;
    }

    public static SlotBase CreateFromEntity(SlotEntity slot, GameToken token)
    {
        if (slot.Type == SlotType.Developer)
        {
            DeveloperSlot devSlot = new()
            {
                SlotId = slot.SlotId,
                InternalSlotId = slot.InternalSlotId,
            };
            return devSlot;
        }

        UserSlot userSlot = new()
        {
            TargetGame = token.GameVersion,
            TargetUserId = token.UserId,
            CreatorId = slot.CreatorId,
            SlotId = slot.SlotId,
            // this gets set in PrepareSerialization
            AuthorHandle = new NpHandle("", ""),
            AuthorLabels = slot.AuthorLabels,
            BackgroundHash = slot.BackgroundHash,
            GameVersion = slot.GameVersion,
            Description = slot.Description,
            Name = slot.Name,
            Location = slot.Location,
            IconHash = slot.IconHash,
            InitiallyLocked = slot.InitiallyLocked,
            RootLevel = slot.RootLevel,
            IsShareable = slot.Shareable,
            IsTeamPicked = slot.TeamPick,
            FirstUploaded = slot.FirstUploaded,
            LastUpdated = slot.LastUpdated,
            IsCrossControlRequired = slot.CrossControllerRequired,
            IsMoveRequired = slot.MoveRequired,
            LevelType = slot.LevelType,
            IsSubLevel = slot.SubLevel,
            MinimumPlayers = slot.MinimumPlayers,
            MaximumPlayers = slot.MaximumPlayers,
            IsAdventurePlanet = slot.IsAdventurePlanet,
            Resources = slot.Resources,
            IsLbp1Only = slot.Lbp1Only,
            CompletePlayCount = slot.PlaysComplete,
            LBP1PlayCount = slot.PlaysLBP1,
            LBP1UniquePlayCount = slot.PlaysLBP1Unique,
            LBP1CompletePlayCount = slot.PlaysLBP1Complete,
            LBP2PlayCount = slot.PlaysLBP2,
            LBP2UniquePlayCount = slot.PlaysLBP2Unique,
            LBP2CompletePlayCount = slot.PlaysLBP2Complete,
            LBP3PlayCount = slot.PlaysLBP3,
            LBP3UniquePlayCount = slot.PlaysLBP3Unique,
            LBP3CompletePlayCount = slot.PlaysLBP3Complete,
        };
        return userSlot;
    }
}

public enum SerializationMode
{
    Full,
    Minimal,
}