using System;
using System.Threading.Tasks;

namespace LBPUnion.ProjectLighthouse.Tests
{
    public class DatabaseTests : LighthouseTest
    {
        [DatabaseFact]
        public async Task CanCreateUserTwice()
        {
            await using Database database = new();
            int rand = new Random().Next();

            await database.CreateUser("createUserTwiceTest" + rand);
            await database.CreateUser("createUserTwiceTest" + rand);
        }
    }
}