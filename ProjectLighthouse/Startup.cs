using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Kettu;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;

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

            services.AddMvc(options => options.OutputFormatters.Add(new XmlOutputFormatter()));

            services.AddDbContext<Database>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            bool computeDigests = true;
            string serverDigestKey = Environment.GetEnvironmentVariable("SERVER_DIGEST_KEY");
            if (string.IsNullOrWhiteSpace(serverDigestKey))
            {
                Logger.Log
                (
                    "The SERVER_DIGEST_KEY environment variable wasn't set, so digest headers won't be set or verified. This will prevent LBP 1 and LBP 3 from working. " +
                    "To increase security, it is recommended that you find and set this variable."
                );
                computeDigests = false;
            }

            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            // Logs every request and the response to it
            // Example: "200, 13ms: GET /LITTLEBIGPLANETPS3_XML/news"
            // Example: "404, 127ms: GET /asdasd?query=osucookiezi727ppbluezenithtopplayhdhr"
            app.Use
            (
                async (context, next) =>
                {
                    Stopwatch requestStopwatch = new();
                    requestStopwatch.Start();

                    // Log all headers.
                    foreach (KeyValuePair<string, StringValues> header in context.Request.Headers) Logger.Log($"{header.Key}: {header.Value}");

                    context.Request.EnableBuffering(); // Allows us to reset the position of Request.Body for later logging

                    // Client digest check.
                    string authCookie;
                    if (!context.Request.Cookies.TryGetValue("MM_AUTH", out authCookie)) authCookie = string.Empty;
                    string digestPath = context.Request.Path;
                    Stream body = context.Request.Body;

                    if (computeDigests)
                    {
                        string clientRequestDigest = await HashHelper.ComputeDigest(digestPath, authCookie, body, serverDigestKey);

                        // Check the digest we've just calculated against the X-Digest-A header if the game set the header. They should match.
                        if (context.Request.Headers.TryGetValue("X-Digest-A", out StringValues sentDigest))
                            if (clientRequestDigest != sentDigest)
                            {
                                context.Response.StatusCode = 403;
                                context.Abort();
                                return;
                            }

                        context.Response.Headers.Add("X-Digest-B", clientRequestDigest);
                        context.Request.Body.Position = 0;
                    }

                    // This does the same as above, but for the response stream.
                    using MemoryStream responseBuffer = new();
                    Stream oldResponseStream = context.Response.Body;
                    context.Response.Body = responseBuffer;

                    await next(); // Handle the request so we can get the status code from it

                    // Compute the server digest hash.
                    if (computeDigests)
                    {
                        responseBuffer.Position = 0;

                        // Compute the digest for the response.
                        string serverDigest = await HashHelper.ComputeDigest(context.Request.Path, authCookie, responseBuffer, serverDigestKey);
                        context.Response.Headers.Add("X-Digest-A", serverDigest);
                    }

                    // Set the X-Original-Content-Length header to the length of the response buffer.
                    context.Response.Headers.Add("X-Original-Content-Length", responseBuffer.Length.ToString());

                    // Copy the buffered response to the actual respose stream.
                    responseBuffer.Position = 0;

                    await responseBuffer.CopyToAsync(oldResponseStream);

                    context.Response.Body = oldResponseStream;

                    requestStopwatch.Stop();

                    Logger.Log
                    (
                        $"{context.Response.StatusCode}, {requestStopwatch.ElapsedMilliseconds}ms: {context.Request.Method} {context.Request.Path}{context.Request.QueryString}",
                        LoggerLevelHttp.Instance
                    );

                    if (context.Request.Method == "POST")
                    {
                        context.Request.Body.Position = 0;
                        Logger.Log(await new StreamReader(context.Request.Body).ReadToEndAsync(), LoggerLevelHttp.Instance);
                    }
                }
            );

            app.UseRouting();

            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}