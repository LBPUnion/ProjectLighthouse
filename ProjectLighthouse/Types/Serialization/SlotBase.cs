using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Levels;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

[XmlInclude(typeof(GameUserSlot))]
[XmlInclude(typeof(GameDeveloperSlot))]
// The C# XML serializer doesn't recognize children of interfaces, only abstract classes. Therefore, this has to be abstract
public abstract class SlotBase : ILbpSerializable
{

    public static SlotBase CreateFromEntity(SlotEntity slot, GameTokenEntity token, SerializationMode serializationMode)
    {
        SlotBase retSlot = CreateFromEntity(slot, token);
        if (retSlot is GameUserSlot userSlot) userSlot.SerializationMode = serializationMode;

        return retSlot;
    }

    public static SlotEntity ConvertToEntity(GameUserSlot slot, GameTokenEntity token) =>
        new()
        {
            Name = slot.Name,
            Description = slot.Description,
            Location = slot.Location,
            IconHash = slot.IconHash,
            BackgroundHash = slot.BackgroundHash,
            AuthorLabels = slot.AuthorLabels,
            Shareable = slot.IsShareable,
            Resources = slot.Resources,
            InitiallyLocked = slot.InitiallyLocked,
            MinimumPlayers = slot.MinimumPlayers,
            MaximumPlayers = slot.MaximumPlayers,
            CreatorId = token.UserId,
            Lbp1Only = slot.IsLbp1Only,
            IsAdventurePlanet = slot.IsAdventurePlanet,
            LevelType = slot.LevelType,
            SubLevel = slot.IsSubLevel,
            RootLevel = slot.RootLevel,
            MoveRequired = slot.IsMoveRequired,
            CrossControllerRequired = slot.IsCrossControlRequired,
        };

    public static SlotBase CreateFromEntity(SlotEntity slot, GameTokenEntity token)
    {
        if (slot.Type == SlotType.Developer)
        {
            GameDeveloperSlot devSlot = new()
            {
                SlotId = slot.SlotId,
                InternalSlotId = slot.InternalSlotId,
            };
            return devSlot;
        }

        GameUserSlot userSlot = new()
        {
            SerializationMode = SerializationMode.Minimal,
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