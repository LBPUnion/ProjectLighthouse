using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Startup;
using LBPUnion.ProjectLighthouse.Tests.Helpers;
using LBPUnion.ProjectLighthouse.Tests.Integration;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Serialization;
using LBPUnion.ProjectLighthouse.Types.Users;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests.Integration;

[Trait("Category", "Integration")]
public class SlotFilterTests : LighthouseServerTest<GameServerTestStartup>
{
    [Fact]
    public async Task GetUserSlot_ShouldReturnOk_WhenSlotExists()
    {
        DatabaseContext db = await IntegrationHelper.GetIntegrationDatabase();

        db.Users.Add(new UserEntity
        {
            UserId = 23,
        });

        db.Slots.Add(new SlotEntity
        {
            SlotId = 23,
            CreatorId = 23,
        });
        await db.SaveChangesAsync();

        LoginResult loginResult = await this.Authenticate();
        HttpResponseMessage response = await this.AuthenticatedRequest("/LITTLEBIGPLANETPS3_XML/s/user/23", loginResult.AuthTicket);

        const HttpStatusCode expectedStatusCode = HttpStatusCode.OK;

        Assert.Equal(expectedStatusCode, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync();

        Assert.Contains("<id>23</id>", body);
    }

    [Fact]
    public async Task NewestSlots_ShouldReturnSlotsOrderedByTimestampDescending()
    {
        DatabaseContext db = await IntegrationHelper.GetIntegrationDatabase();

        for (int i = 1; i <= 100; i++)
        {
            db.Users.Add(new UserEntity
            {
                UserId = i,
                Username = $"user{i}",
            });
            db.Slots.Add(new SlotEntity
            {
                SlotId = i,
                CreatorId = i,
                FirstUploaded = i,
            });
        }
        await db.SaveChangesAsync();

        LoginResult loginResult = await this.Authenticate();
        HttpResponseMessage response =
            await this.AuthenticatedRequest("/LITTLEBIGPLANETPS3_XML/slots?pageStart=0&pageSize=10", loginResult.AuthTicket);

        const HttpStatusCode expectedStatusCode = HttpStatusCode.OK;

        Assert.Equal(expectedStatusCode, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync();

        object? deserialized = LighthouseSerializer
            .GetSerializer(typeof(GameUserSlotList), new XmlRootAttribute("slots"))
            .Deserialize(new StringReader(body));
        Assert.NotNull(deserialized);
        Assert.IsType<GameUserSlotList>(deserialized);

        GameUserSlotList slotResponse = (GameUserSlotList)deserialized;

        Assert.Equal(100, slotResponse.Total);
        Assert.Equal(10, slotResponse.Slots.Count);

        Assert.Equal(91, slotResponse.Slots[9].FirstUploaded);
    }

    [Fact]
    public async Task NewestSlots_ShouldReturnSlotsWithAuthorLabel()
    {
        DatabaseContext db = await IntegrationHelper.GetIntegrationDatabase();

        db.Users.Add(new UserEntity()
        {
            UserId = 1,
        });

        db.Slots.Add(new SlotEntity
        {
            CreatorId = 1,
            SlotId = 1,
            AuthorLabels = "LABEL_SinglePlayer,LABEL_Quick,LABEL_Funny",
            FirstUploaded = 1,
        });
        db.Slots.Add(new SlotEntity
        {
            CreatorId = 1,
            SlotId = 2,
            AuthorLabels = "LABEL_SinglePlayer",
            FirstUploaded = 2,
        });
        db.Slots.Add(new SlotEntity
        {
            CreatorId = 1,
            SlotId = 3,
            AuthorLabels = "LABEL_Quick",
            FirstUploaded = 3,
        });
        db.Slots.Add(new SlotEntity
        {
            CreatorId = 1,
            SlotId = 4,
            AuthorLabels = "LABEL_Funny",
            FirstUploaded = 4,
        });

        await db.SaveChangesAsync();

        LoginResult loginResult = await this.Authenticate();
        HttpResponseMessage response =
            await this.AuthenticatedRequest("/LITTLEBIGPLANETPS3_XML/slots?pageStart=0&pageSize=10&labelFilter0=LABEL_Funny",
                loginResult.AuthTicket);

        const HttpStatusCode expectedStatusCode = HttpStatusCode.OK;

        Assert.Equal(expectedStatusCode, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync();

        object? deserialized = LighthouseSerializer
            .GetSerializer(typeof(GameUserSlotList), new XmlRootAttribute("slots"))
            .Deserialize(new StringReader(body));
        Assert.NotNull(deserialized);
        Assert.IsType<GameUserSlotList>(deserialized);

        GameUserSlotList slotResponse = (GameUserSlotList)deserialized;

        const int expectedCount = 2;

        Assert.Equal(expectedCount, slotResponse.Slots.Count);
        Assert.True(slotResponse.Slots.TrueForAll(s => s.AuthorLabels.Contains("LABEL_Funny")));
    }

[Fact]
    public async Task NewestSlots_ShouldReturnEmpty_WhenAuthorLabelsDontMatch()
    {
        DatabaseContext db = await IntegrationHelper.GetIntegrationDatabase();

        db.Users.Add(new UserEntity
        {
            UserId = 1,
        });

        db.Slots.Add(new SlotEntity
        {
            CreatorId = 1,
            SlotId = 1,
            AuthorLabels = "LABEL_SinglePlayer,LABEL_Quick,LABEL_Funny",
            FirstUploaded = 1,
        });
        db.Slots.Add(new SlotEntity
        {
            CreatorId = 1,
            SlotId = 2,
            AuthorLabels = "LABEL_SinglePlayer",
            FirstUploaded = 2,
        });
        db.Slots.Add(new SlotEntity
        {
            CreatorId = 1,
            SlotId = 3,
            AuthorLabels = "LABEL_Quick",
            FirstUploaded = 3,
        });
        db.Slots.Add(new SlotEntity
        {
            CreatorId = 1,
            SlotId = 4,
            AuthorLabels = "LABEL_Funny",
            FirstUploaded = 4,
        });

        await db.SaveChangesAsync();

        LoginResult loginResult = await this.Authenticate();
        HttpResponseMessage response =
            await this.AuthenticatedRequest("/LITTLEBIGPLANETPS3_XML/slots?pageStart=0&pageSize=10&labelFilter0=LABEL_Funny&labelFilter1=LABEL_Quick&labelFilter2=LABEL_Gallery",
                loginResult.AuthTicket);

        const HttpStatusCode expectedStatusCode = HttpStatusCode.OK;

        Assert.Equal(expectedStatusCode, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync();

        object? deserialized = LighthouseSerializer
            .GetSerializer(typeof(GameUserSlotList), new XmlRootAttribute("slots"))
            .Deserialize(new StringReader(body));
        Assert.NotNull(deserialized);
        Assert.IsType<GameUserSlotList>(deserialized);

        GameUserSlotList slotResponse = (GameUserSlotList)deserialized;

        Assert.Empty(slotResponse.Slots);
    }

    [XmlRoot("slots")]
    public class GameUserSlotList
    {
        [XmlElement("slot")]
        public List<GameUserSlot> Slots { get; set; } = new();

        [XmlAttribute("total")]
        public int Total { get; set; }

        [XmlAttribute("hint_start")]
        public int HintStart { get; set; }
    }
}