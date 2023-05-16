using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
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
using Xunit;

namespace LBPUnion.ProjectLighthouse.Tests.Integration;

[Collection(nameof(LighthouseServerTest<TStartup>))]
public class LighthouseServerTest<TStartup> where TStartup : class
{
    protected readonly HttpClient Client;
    private readonly TestServer server;

    protected LighthouseServerTest()
    {
        ServerConfiguration.Instance.DbConnectionString = "server=127.0.0.1;uid=root;pwd=lighthouse_tests;database=lighthouse_tests";
        ServerConfiguration.Instance.DigestKey.PrimaryDigestKey = "lighthouse";
        this.server = new TestServer(new WebHostBuilder().UseStartup<TStartup>());
        this.Client = this.server.CreateClient();
    }

    public TestServer GetTestServer() => this.server;

    protected async Task<UserEntity> CreateRandomUser()
    {
        await using DatabaseContext database = DatabaseContext.CreateNewInstance();

        int userId = RandomNumberGenerator.GetInt32(int.MaxValue);
        const string username = "unitTestUser";
        // if user already exists, find another random number
        while (await database.Users.AnyAsync(u => u.Username == $"{username}{userId}"))
        {
            userId = RandomNumberGenerator.GetInt32(int.MaxValue);
        }

        UserEntity user = new()
        {
            UserId = userId,
            Username = $"{username}{userId}",
            Password = CryptoHelper.BCryptHash($"unitTestPassword{userId}"),
            LinkedPsnId = (ulong)userId,
        };

        database.Add(user);
        await database.SaveChangesAsync();
        return user;
    }

    protected async Task<HttpResponseMessage> AuthenticateResponse()
    {
        UserEntity user = await this.CreateRandomUser();

        byte[] ticketData = new TicketBuilder()
            .SetUsername($"{user.Username}{user.UserId}")
            .SetUserId((ulong)user.UserId)
            .Build();

        HttpResponseMessage response = await this.Client.PostAsync
            ($"/LITTLEBIGPLANETPS3_XML/login?titleID={GameVersionHelper.LittleBigPlanet2TitleIds[0]}", new ByteArrayContent(ticketData));
        return response;
    }

    protected async Task<LoginResult> Authenticate()
    {
        HttpResponseMessage response = await this.AuthenticateResponse();

        string responseContent = await response.Content.ReadAsStringAsync();

        XmlSerializer serializer = new(typeof(LoginResult));
        return (LoginResult)serializer.Deserialize(new StringReader(responseContent))!;
    }

    protected Task<HttpResponseMessage> AuthenticatedRequest(string endpoint, string mmAuth) => this.AuthenticatedRequest(endpoint, mmAuth, HttpMethod.Get);

    private static string GetDigestCookie(string mmAuth) => mmAuth["MM_AUTH=".Length..];

    private Task<HttpResponseMessage> AuthenticatedRequest(string endpoint, string mmAuth, HttpMethod method)
    {
        using HttpRequestMessage requestMessage = new(method, endpoint);
        requestMessage.Headers.Add("Cookie", mmAuth);
        string path = endpoint.Split("?", StringSplitOptions.RemoveEmptyEntries)[0];

        string digest = CryptoHelper.ComputeDigest(path, GetDigestCookie(mmAuth), Array.Empty<byte>(), "lighthouse");
        requestMessage.Headers.Add("X-Digest-A", digest);

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
        string digest = CryptoHelper.ComputeDigest($"/LITTLEBIGPLANETPS3_XML/upload/{hash}", GetDigestCookie(mmAuth), bytes, "lighthouse", true);
        requestMessage.Headers.Add("X-Digest-B", digest);
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
        string digest = CryptoHelper.ComputeDigest(endpoint, GetDigestCookie(mmAuth), data, "lighthouse");
        requestMessage.Headers.Add("X-Digest-A", digest);
        return await this.Client.SendAsync(requestMessage);
    }
}