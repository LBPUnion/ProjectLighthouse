using System;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Types;

namespace LBPUnion.ProjectLighthouse.Tests
{
    public class DatabaseTests : LighthouseTest
    {
        [DatabaseFact]
        public async Task CanCreateUserTwice()
        {
            await using Database database = new();
            int rand = new Random().Next();

            User userA = await database.CreateUser("createUserTwiceTest" + rand);
            User userB = await database.CreateUser("createUserTwiceTest" + rand);

            database.Users.Remove(userA);
            database.Users.Remove(userB);

            await database.SaveChangesAsync();
        }
    }
}