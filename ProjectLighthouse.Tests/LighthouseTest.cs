using System;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
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
    }
}