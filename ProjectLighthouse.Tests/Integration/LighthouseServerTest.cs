using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Tickets;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Tests.Integration;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class LighthouseServerTest<TStartup> where TStartup : class
{
    protected readonly HttpClient Client;
    private readonly TestServer server;

    protected LighthouseServerTest()
    {
        ServerConfiguration.Instance.DbConnectionString = "server=127.0.0.1;uid=root;pwd=lighthouse_tests;database=lighthouse_tests";
        this.server = new TestServer(new WebHostBuilder().UseStartup<TStartup>());
        this.Client = this.server.CreateClient();
    }

    public TestServer GetTestServer() => this.server;

    protected async Task<string> CreateRandomUser(int number = -1, bool createUser = true)
    {
        if (number == -1) number = new Random().Next();
        const string username = "unitTestUser";

        if (createUser)
        {
            await using DatabaseContext database = new();
            if (await database.Users.FirstOrDefaultAsync(u => u.Username == $"{username}{number}") == null)
            {
                UserEntity user = await database.CreateUser($"{username}{number}",
                    CryptoHelper.BCryptHash($"unitTestPassword{number}"));
                user.LinkedPsnId = (ulong)number;
                await database.SaveChangesAsync();
            }
        }

        return $"{username}{number}";
    }

    protected async Task<HttpResponseMessage> AuthenticateResponse(int number = -1, bool createUser = true)
    {
        string username = await this.CreateRandomUser(number, createUser);

        byte[] ticketData = new TicketBuilder()
            .SetUsername($"{username}{number}")
            .SetUserId((ulong)number)
            .Build();

        HttpResponseMessage response = await this.Client.PostAsync
            ($"/LITTLEBIGPLANETPS3_XML/login?titleID={GameVersionHelper.LittleBigPlanet2TitleIds[0]}", new ByteArrayContent(ticketData));
        return response;
    }

    protected async Task<LoginResult> Authenticate(int number = -1)
    {
        HttpResponseMessage response = await this.AuthenticateResponse(number);

        string responseContent = await response.Content.ReadAsStringAsync();

        XmlSerializer serializer = new(typeof(LoginResult));
        return (LoginResult)serializer.Deserialize(new StringReader(responseContent))!;
    }

    protected Task<HttpResponseMessage> AuthenticatedRequest(string endpoint, string mmAuth) => this.AuthenticatedRequest(endpoint, mmAuth, HttpMethod.Get);

    private Task<HttpResponseMessage> AuthenticatedRequest(string endpoint, string mmAuth, HttpMethod method)
    {
        using HttpRequestMessage requestMessage = new(method, endpoint);
        requestMessage.Headers.Add("Cookie", mmAuth);

        return this.Client.SendAsync(requestMessage);
    }

    public async Task<HttpResponseMessage> UploadFileEndpointRequest(string filePath)
    {
        byte[] bytes = await File.ReadAllBytesAsync(filePath);
        string hash = CryptoHelper.Sha1Hash(bytes).ToLower();

        return await this.Client.PostAsync($"/LITTLEBIGPLANETPS3_XML/upload/{hash}", new ByteArrayContent(bytes));
    }

    protected async Task<HttpResponseMessage> AuthenticatedUploadFileEndpointRequest(string filePath, string mmAuth)
    {
        byte[] bytes = await File.ReadAllBytesAsync(filePath);
        string hash = CryptoHelper.Sha1Hash(bytes).ToLower();
        using HttpRequestMessage requestMessage = new(HttpMethod.Post, $"/LITTLEBIGPLANETPS3_XML/upload/{hash}");
        requestMessage.Headers.Add("Cookie", mmAuth);
        requestMessage.Content = new ByteArrayContent(bytes);
        return await this.Client.SendAsync(requestMessage);
    }

    public async Task<HttpResponseMessage> UploadFileRequest(string endpoint, string filePath)
        => await this.Client.PostAsync(endpoint, new StringContent(await File.ReadAllTextAsync(filePath)));

    public async Task<HttpResponseMessage> UploadDataRequest(string endpoint, byte[] data) => await this.Client.PostAsync(endpoint, new ByteArrayContent(data));

    public async Task<HttpResponseMessage> AuthenticatedUploadFileRequest(string endpoint, string filePath, string mmAuth)
    {
        using HttpRequestMessage requestMessage = new(HttpMethod.Post, endpoint);
        requestMessage.Headers.Add("Cookie", mmAuth);
        requestMessage.Content = new StringContent(await File.ReadAllTextAsync(filePath));
        return await this.Client.SendAsync(requestMessage);
    }

    protected async Task<HttpResponseMessage> AuthenticatedUploadDataRequest(string endpoint, byte[] data, string mmAuth)
    {
        using HttpRequestMessage requestMessage = new(HttpMethod.Post, endpoint);
        requestMessage.Headers.Add("Cookie", mmAuth);
        requestMessage.Content = new ByteArrayContent(data);
        return await this.Client.SendAsync(requestMessage);
    }
}