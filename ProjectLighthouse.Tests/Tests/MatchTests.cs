using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Types;
using Xunit;

namespace LBPUnion.ProjectLighthouse.Tests
{
    public class MatchTests : LighthouseTest
    {
        private static readonly SemaphoreSlim semaphore = new(1, 1);

        [DatabaseFact]
        public async Task ShouldRejectEmptyData()
        {
            LoginResult loginResult = await this.Authenticate();
            await semaphore.WaitAsync();

            HttpResponseMessage result = await this.AuthenticatedUploadDataRequest("LITTLEBIGPLANETPS3_XML/match", Array.Empty<byte>(), loginResult.AuthTicket);

            semaphore.Release();
            Assert.False(result.IsSuccessStatusCode);
        }

        [DatabaseFact]
        public async Task ShouldReturnOk()
        {
            LoginResult loginResult = await this.Authenticate();
            await semaphore.WaitAsync();

            HttpResponseMessage result = await this.AuthenticatedUploadDataRequest
                ("LITTLEBIGPLANETPS3_XML/match", Encoding.ASCII.GetBytes("[UpdateMyPlayerData,[\"Player\":\"1984\"]]"), loginResult.AuthTicket);

            semaphore.Release();
            Assert.True(result.IsSuccessStatusCode);
        }
        public async Task<int> GetPlayerCount() => Convert.ToInt32(await this.Client.GetStringAsync("LITTLEBIGPLANETPS3_XML/totalPlayerCount"));

        [DatabaseFact]
        public async Task ShouldIncrementPlayerCount()
        {
            LoginResult loginResult = await this.Authenticate(new Random().Next());

            await semaphore.WaitAsync();

            int oldPlayerCount = await this.GetPlayerCount();

            HttpResponseMessage result = await this.AuthenticatedUploadDataRequest
                ("LITTLEBIGPLANETPS3_XML/match", Encoding.ASCII.GetBytes("[UpdateMyPlayerData,[\"Player\":\"1984\"]]"), loginResult.AuthTicket);

            Assert.True(result.IsSuccessStatusCode);

            int playerCount = await this.GetPlayerCount();

            semaphore.Release();
            Assert.Equal(oldPlayerCount + 1, playerCount);
        }
    }
}