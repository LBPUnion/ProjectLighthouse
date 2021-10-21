using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Types;
using Xunit;

namespace LBPUnion.ProjectLighthouse.Tests {
    public class MatchTests : LighthouseTest {
        private static readonly SemaphoreSlim semaphore = new(1, 1);

        [DatabaseFact]
        public async Task ShouldReturnOk() {
            LoginResult loginResult = await this.Authenticate();
            await semaphore.WaitAsync();

            HttpResponseMessage result = await this.AuthenticatedUploadDataRequest("LITTLEBIGPLANETPS3_XML/match", Array.Empty<byte>(), loginResult.AuthTicket);
            Assert.True(result.IsSuccessStatusCode);

            semaphore.Release();
        }
        public async Task<int> GetPlayerCount() => Convert.ToInt32(await this.Client.GetStringAsync("LITTLEBIGPLANETPS3_XML/totalPlayerCount"));

        [DatabaseFact]
        public async Task ShouldIncrementPlayerCount() {
            LoginResult loginResult = await this.Authenticate(new Random().Next());

            await semaphore.WaitAsync();

            int oldPlayerCount = await this.GetPlayerCount();

            HttpResponseMessage result = await this.AuthenticatedUploadDataRequest("LITTLEBIGPLANETPS3_XML/match", Array.Empty<byte>(), loginResult.AuthTicket);
            Assert.True(result.IsSuccessStatusCode);

            int playerCount = await this.GetPlayerCount();
            
            semaphore.Release();
            Assert.Equal(oldPlayerCount + 1, playerCount);
        }
    }
}