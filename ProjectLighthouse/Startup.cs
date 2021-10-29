using System;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Kettu;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LBPUnion.ProjectLighthouse
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddMvc(options =>
                options.OutputFormatters.Add(new XmlOutputFormatter()));

            services.AddDbContext<Database>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var computeDigests = true;
            var serverDigestKey = Environment.GetEnvironmentVariable("SERVER_DIGEST_KEY");
            if (string.IsNullOrWhiteSpace(serverDigestKey))
            {
                Logger.Log(
                    "SERVER_DIGEST_KEY environment variable wasn't set, so server digest headers won't be set. This will break LBP 1 and LBP 3."
                );
                computeDigests = false;
            }
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Logs every request and the response to it
            // Example: "200, 13ms: GET /LITTLEBIGPLANETPS3_XML/news"
            // Example: "404, 127ms: GET /asdasd?query=osucookiezi727ppbluezenithtopplayhdhr"
            app.Use(async (context, next) =>
            {
                Stopwatch requestStopwatch = new();
                requestStopwatch.Start();

                context.Request.EnableBuffering(); // Allows us to reset the position of Request.Body for later logging

                // Client digest check.
                var authCookie = null as string;
                if (!context.Request.Cookies.TryGetValue("MM_AUTH", out authCookie))
                    authCookie = string.Empty;
                if (context.Request.Headers.TryGetValue("X-Digest-A", out var clientDigest))
                {
                    var digestPath = context.Request.Path;
                    var body = context.Request.Body;

                    var digest = await DigestUtils.ComputeDigest(digestPath, authCookie, body, serverDigestKey);

                    if (digest != clientDigest)
                    {
                        Logger.Log($"Client digest {clientDigest} does not match server digest {digest}.");
                        context.Abort();
                        return;
                    }
                    else
                    {
                        context.Response.Headers.Add("X-Digest-B", digest);
                        context.Request.Body.Position = 0;
                    }
                }

                // This does the same as above, but for the response stream.
                using var responseBuffer = new MemoryStream();
                var oldResponseStream = context.Response.Body;
                context.Response.Body = responseBuffer;

                await next(); // Handle the request so we can get the status code from it

                // Compute the server digest hash.
                if (computeDigests && context.Request.Headers.TryGetValue("X-Digest-A", out var a))
                {
                    responseBuffer.Position = 0;
                    
                    // Compute the digest for the response.
                    var serverDigest = await DigestUtils.ComputeDigest(context.Request.Path, authCookie,
                        responseBuffer, serverDigestKey);
                    context.Response.Headers.Add("X-Digest-A", serverDigest);
                }

                // Copy the buffered response to the actual respose stream.
                responseBuffer.Position = 0;
                
                await responseBuffer.CopyToAsync(oldResponseStream);
                
                context.Response.Body = oldResponseStream;

                requestStopwatch.Stop();

                Logger.Log(
                    $"{context.Response.StatusCode}, {requestStopwatch.ElapsedMilliseconds}ms: {context.Request.Method} {context.Request.Path}{context.Request.QueryString}",
                    LoggerLevelHttp.Instance
                );

                if (context.Request.Method == "POST")
                {
                    context.Request.Body.Position = 0;
                    Logger.Log(await new StreamReader(context.Request.Body).ReadToEndAsync(), LoggerLevelHttp.Instance);
                }
            });

            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}