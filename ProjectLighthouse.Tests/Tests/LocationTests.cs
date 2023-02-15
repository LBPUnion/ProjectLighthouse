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
    public void CanSetAndReadLocation()
    {
        Location expected = new()
        {
            X = 1000,
            Y = 5000,
        };
        User user = new()
        {
            Location = new Location
            {
                X = expected.X,
                Y = expected.Y,
            },
        };
        Assert.True(user.Location.X == expected.X);
        Assert.True(user.Location.Y == expected.Y);
        Assert.True(user.LocationPacked != 0);
    }

    [Fact]
    public void DeserializedLocationIsSet()
    {
        XmlSerializer deserializer = new(typeof(Slot));
        const string slotData = "<slot><name>test</name><resource>test</resource><location><x>4000</x><y>9000</y></location></slot>";

        Slot? deserialized = (Slot?)deserializer.Deserialize(new StringReader(slotData));
        Assert.True(deserialized != null);
        Assert.True(deserialized.Name == "test");
        Assert.True(deserialized.Location.X == 4000);
        Assert.True(deserialized.Location.Y == 9000);
    }

    [Fact]
    public void ShouldDeserializeEmptyLocation()
    {
        XmlSerializer deserializer = new(typeof(Slot));
        const string slotData =
            "<slot><name>test</name></slot>";

        Slot? deserialized = (Slot?)deserializer.Deserialize(new StringReader(slotData));
        Assert.True(deserialized != null);
        Assert.True(deserialized.Name == "test");
        Assert.True(deserialized.Location.X == 0);
        Assert.True(deserialized.Location.Y == 0);
    }

}