using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ProjectLighthouse.Types;
using Xunit;

namespace ProjectLighthouse.Tests {
    public class AuthenticationTest : LighthouseTest {
        [Fact]
        public async Task ShouldReturnErrorOnNoPostData() {
            HttpResponseMessage response = await this.Client.PostAsync("/LITTLEBIGPLANETPS3_XML/login", null!);
            Assert.False(response.IsSuccessStatusCode);
            #if NET6_0_OR_GREATER
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
            #else
            Assert.True(response.StatusCode == HttpStatusCode.NotAcceptable);
            #endif
        }

        [DatabaseFact]
        public async Task ShouldReturnWithValidData() {
            HttpResponseMessage response = await this.AuthenticateResponse();
            Assert.True(response.IsSuccessStatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("MM_AUTH=", responseContent);
            Assert.Contains(ServerSettings.ServerName, responseContent);
        }

        [DatabaseFact]
        public async Task CanSerializeBack() {
            LoginResult loginResult = await this.Authenticate();
            
            Assert.NotNull(loginResult);
            Assert.NotNull(loginResult.AuthTicket);
            Assert.NotNull(loginResult.LbpEnvVer);

            Assert.Contains("MM_AUTH=", loginResult.AuthTicket);
            Assert.Equal(ServerSettings.ServerName, loginResult.LbpEnvVer);
        }

        [DatabaseFact]
        public async Task CanUseToken() {
            LoginResult loginResult = await this.Authenticate();

            HttpResponseMessage response = await AuthenticatedRequest("/LITTLEBIGPLANETPS3_XML/eula", loginResult.AuthTicket);
            string responseContent = await response.Content.ReadAsStringAsync();
            
            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains("You are logged in", responseContent);
        }

        [DatabaseFact]
        public async Task ShouldReturnForbiddenWhenNotAuthenticated() {
            HttpResponseMessage response = await this.Client.GetAsync("/LITTLEBIGPLANETPS3_XML/eula");
            Assert.False(response.IsSuccessStatusCode);
            Assert.True(response.StatusCode == HttpStatusCode.Forbidden);
        }
    }
}