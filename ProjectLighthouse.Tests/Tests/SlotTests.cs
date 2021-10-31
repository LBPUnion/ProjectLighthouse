using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Profiles;
using Xunit;

namespace LBPUnion.ProjectLighthouse.Tests
{
    public class SlotTests : LighthouseTest
    {
        [DatabaseFact]
        public async Task ShouldOnlyShowUsersLevels()
        {
            await using Database database = new();

            User userA = await database.CreateUser("unitTestUser0");
            User userB = await database.CreateUser("unitTestUser1");

            Location l = new();
            database.Locations.Add(l);
            await database.SaveChangesAsync();

            Slot slotA = new()
            {
                Creator = userA,
                Name = "slotA",
                Location = l,
                LocationId = l.Id,
                ResourceCollection = "",
            };

            Slot slotB = new()
            {
                Creator = userB,
                Name = "slotB",
                Location = l,
                LocationId = l.Id,
                ResourceCollection = "",
            };

            database.Slots.Add(slotA);
            database.Slots.Add(slotB);

            await database.SaveChangesAsync();

//            XmlSerializer serializer = new(typeof(Slot));
//            Slot slot = (Slot)serializer.Deserialize(new StringReader(bodyString));

            string respA = await this.Client.GetStringAsync("LITTLEBIGPLANETPS3_XML/slots/by?u=unitTestUser0");
            string respB = await this.Client.GetStringAsync("LITTLEBIGPLANETPS3_XML/slots/by?u=unitTestUser1");

            Assert.NotEqual(respA, respB);
            Assert.DoesNotContain(respA, "slotB");
            Assert.DoesNotContain(respB, "slotA");

            // Cleanup

            database.Slots.Remove(slotA);
            database.Slots.Remove(slotB);
        }
    }
}