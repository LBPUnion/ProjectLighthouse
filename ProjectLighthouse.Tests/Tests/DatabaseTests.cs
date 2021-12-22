using System;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Types;
using Xunit;

namespace LBPUnion.ProjectLighthouse.Tests
{
    public class DatabaseTests : LighthouseTest
    {
        [DatabaseFact]
        public async Task CanCreateUserTwice()
        {
            await using Database database = new();
            int rand = new Random().Next();

            User userA = await database.CreateUser("createUserTwiceTest" + rand, HashHelper.GenerateAuthToken());
            User userB = await database.CreateUser("createUserTwiceTest" + rand, HashHelper.GenerateAuthToken());

            Assert.NotNull(userA);
            Assert.NotNull(userB);

            await database.RemoveUser(userA); // Only remove userA since userA and userB are the same user

            await database.SaveChangesAsync();
        }
    }
}