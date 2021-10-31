using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace LBPUnion.ProjectLighthouse.Tests
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class LighthouseTest
    {
        public readonly HttpClient Client;
        public readonly TestServer Server;

        public LighthouseTest()
        {
            this.Server = new TestServer(new WebHostBuilder().UseStartup<Startup>());

            this.Client = this.Server.CreateClient();
        }

        public async Task<HttpResponseMessage> AuthenticateResponse(int number = 0)
        {
            const string username = "unitTestUser";

            string stringContent = $"{LoginData.UsernamePrefix}{username}{number}{(char)0x00}";

            HttpResponseMessage response = await this.Client.PostAsync("/LITTLEBIGPLANETPS3_XML/login", new StringContent(stringContent));
            return response;
        }

        public async Task<LoginResult> Authenticate(int number = 0)
        {
            HttpResponseMessage response = await this.AuthenticateResponse(number);

            string responseContent = LbpSerializer.StringElement("loginResult", await response.Content.ReadAsStringAsync());

            XmlSerializer serializer = new(typeof(LoginResult));
            return (LoginResult)serializer.Deserialize(new StringReader(responseContent))!;
        }

        public Task<HttpResponseMessage> AuthenticatedRequest(string endpoint, string mmAuth) => this.AuthenticatedRequest(endpoint, mmAuth, HttpMethod.Get);

        public Task<HttpResponseMessage> AuthenticatedRequest(string endpoint, string mmAuth, HttpMethod method)
        {
            using HttpRequestMessage? requestMessage = new(method, endpoint);
            requestMessage.Headers.Add("Cookie", mmAuth);

            return this.Client.SendAsync(requestMessage);
        }

        public async Task<HttpResponseMessage> UploadFileRequest(string endpoint, string filePath)
            => await this.Client.PostAsync(endpoint, new StringContent(await File.ReadAllTextAsync(filePath)));

        public async Task<HttpResponseMessage> UploadDataRequest(string endpoint, byte[] data)
            => await this.Client.PostAsync(endpoint, new ByteArrayContent(data));

        public async Task<HttpResponseMessage> AuthenticatedUploadFileRequest(string endpoint, string filePath, string mmAuth)
        {
            using HttpRequestMessage? requestMessage = new(HttpMethod.Post, endpoint);
            requestMessage.Headers.Add("Cookie", mmAuth);
            requestMessage.Content = new StringContent(await File.ReadAllTextAsync(filePath));
            return await this.Client.SendAsync(requestMessage);
        }

        public async Task<HttpResponseMessage> AuthenticatedUploadDataRequest(string endpoint, byte[] data, string mmAuth)
        {
            using HttpRequestMessage? requestMessage = new(HttpMethod.Post, endpoint);
            requestMessage.Headers.Add("Cookie", mmAuth);
            requestMessage.Content = new ByteArrayContent(data);
            return await this.Client.SendAsync(requestMessage);
        }
    }
}