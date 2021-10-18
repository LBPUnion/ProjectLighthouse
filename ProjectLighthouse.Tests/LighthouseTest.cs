using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using ProjectLighthouse.Serialization;
using ProjectLighthouse.Types;

namespace ProjectLighthouse.Tests {
    public class LighthouseTest {
        public readonly TestServer Server;
        public readonly HttpClient Client;

        public LighthouseTest() {
            this.Server = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>());
            
            this.Client = this.Server.CreateClient();
        }

        public async Task<HttpResponseMessage> AuthenticateResponse(int number = 0) {
            const char nullChar = (char)0x00;
            const char sepChar = (char)0x20;
            const string username = "unitTestUser";

            string stringContent = $"{nullChar}{sepChar}{username}{number}{nullChar}";
            
            HttpResponseMessage response = await this.Client.PostAsync("/LITTLEBIGPLANETPS3_XML/login", new StringContent(stringContent));
            return response;
        }
        
        public async Task<LoginResult> Authenticate(int number = 0) {
            HttpResponseMessage response = await this.AuthenticateResponse(number);

            string responseContent = LbpSerializer.StringElement("loginResult", await response.Content.ReadAsStringAsync());

            XmlSerializer serializer = new(typeof(LoginResult));
            return (LoginResult)serializer.Deserialize(new StringReader(responseContent))!;
        }

        public Task<HttpResponseMessage> AuthenticatedRequest(string endpoint, string mmAuth) => AuthenticatedRequest(endpoint, mmAuth, HttpMethod.Get);

        public Task<HttpResponseMessage> AuthenticatedRequest(string endpoint, string mmAuth, HttpMethod method) {
            using var requestMessage = new HttpRequestMessage(method, endpoint);
            requestMessage.Headers.Add("Cookie", mmAuth);

            return this.Client.SendAsync(requestMessage);
        }

        public async Task<HttpResponseMessage> UploadFileRequest(string endpoint, string filePath) {
            return await this.Client.PostAsync(endpoint, new StringContent(await File.ReadAllTextAsync(filePath)));
        }

        public async Task<HttpResponseMessage> AuthenticatedUploadFileRequest(string endpoint, string filePath) {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint);
            requestMessage.Content = new StringContent(await File.ReadAllTextAsync(filePath));
            return await this.Client.SendAsync(requestMessage);
        }
    }
}