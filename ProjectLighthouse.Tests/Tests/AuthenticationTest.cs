using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace ProjectLighthouse.Tests {
    public class AuthenticationTest : LighthouseTest {
        [Fact]
        public async Task ShouldReturnErrorOnNoPostData() {
            var response = await this.Client.PostAsync("/LITTLEBIGPLANETPS3_XML/login", null!);
            Assert.False(response.IsSuccessStatusCode);
            #if NET6_0_OR_GREATER
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
            #else
            Assert.True(response.StatusCode == HttpStatusCode.NotAcceptable);
            #endif
        }

        [DatabaseFact]
        public async Task ShouldAuthenticateWithValidData() {
            const char nullChar = (char)0x00;
            const char sepChar = (char)0x20; 
            
            var response = await this.Client.PostAsync("/LITTLEBIGPLANETPS3_XML/login", new StringContent($"{nullChar}{sepChar}jvyden{nullChar}"));
            Assert.True(response.StatusCode == HttpStatusCode.OK);
        }
    }
}