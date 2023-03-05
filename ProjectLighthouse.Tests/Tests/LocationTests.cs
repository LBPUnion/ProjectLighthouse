using System.IO;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Misc;
using Xunit;

namespace LBPUnion.ProjectLighthouse.Tests;

public class LocationTests
{
    [Fact]
    public void ShouldSetAndReadUserLocation()
    {
        Location expected = new()
        {
            X = 1000,
            Y = 5000,
        };
        UserEntity user = new()
        {
            Location = new Location
            {
                X = expected.X,
                Y = expected.Y,
            },
        };
        Assert.True(user.Location.X == expected.X);
        Assert.True(user.Location.Y == expected.Y);
        Assert.True(user.LocationPacked == 4_294_967_301_000);
    }

    [Fact]
    public void ShouldSetAndReadSlotLocation()
    {
        Location expected = new()
        {
            X = 1000,
            Y = 5000,
        };
        SlotEntity slot = new()
        {
            Location = new Location
            {
                X = expected.X,
                Y = expected.Y,
            },
        };
        Assert.True(slot.Location.X == expected.X);
        Assert.True(slot.Location.Y == expected.Y);
        Assert.True(slot.LocationPacked == 4_294_967_301_000);
    }

    [Fact]
    public void ShouldReadLocationAfterDeserialization()
    {
        XmlSerializer deserializer = new(typeof(SlotEntity));
        const string slotData = "<slot><name>test</name><resource>test</resource><location><x>4000</x><y>9000</y></location></slot>";

        SlotEntity? deserialized = (SlotEntity?)deserializer.Deserialize(new StringReader(slotData));
        Assert.True(deserialized != null);
        Assert.True(deserialized.Name == "test");
        Assert.True(deserialized.Location.X == 4000);
        Assert.True(deserialized.Location.Y == 9000);
        Assert.True(deserialized.LocationPacked == 17_179_869_193_000);
    }

    [Fact]
    public void ShouldDeserializeEmptyLocation()
    {
        XmlSerializer deserializer = new(typeof(SlotEntity));
        const string slotData = "<slot><name>test</name></slot>";

        SlotEntity? deserialized = (SlotEntity?)deserializer.Deserialize(new StringReader(slotData));
        Assert.True(deserialized != null);
        Assert.True(deserialized.Name == "test");
        Assert.True(deserialized.Location.X == 0);
        Assert.True(deserialized.Location.Y == 0);
        Assert.True(deserialized.LocationPacked == 0);
    }
}