using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ProjectLighthouse.Types;
using Xunit;
using Xunit.Abstractions;

namespace ProjectLighthouse.Tests {
    public class MatchTests : LighthouseTest {
        private static SemaphoreSlim semaphore = new(1, 1);

        [DatabaseFact]
        public async Task ShouldReturnOk() {
            LoginResult loginResult = await this.Authenticate();
            await semaphore.WaitAsync();

            HttpResponseMessage result = await AuthenticatedUploadDataRequest("LITTLEBIGPLANETPS3_XML/match", Array.Empty<byte>(), loginResult.AuthTicket);
            Assert.True(result.IsSuccessStatusCode);

            semaphore.Release();
        }
        public async Task<int> GetPlayerCount() => Convert.ToInt32(await this.Client.GetStringAsync("LITTLEBIGPLANETPS3_XML/totalPlayerCount"));

        [DatabaseFact]
        public async Task ShouldIncrementPlayerCount() {
            LoginResult loginResult = await this.Authenticate(new Random().Next());

            await semaphore.WaitAsync();

            int oldPlayerCount = await this.GetPlayerCount();

            HttpResponseMessage result = await AuthenticatedUploadDataRequest("LITTLEBIGPLANETPS3_XML/match", Array.Empty<byte>(), loginResult.AuthTicket);
            Assert.True(result.IsSuccessStatusCode);

            int playerCount = await this.GetPlayerCount();
            
            semaphore.Release();
            Assert.Equal(oldPlayerCount + 1, playerCount);
        }
    }
}