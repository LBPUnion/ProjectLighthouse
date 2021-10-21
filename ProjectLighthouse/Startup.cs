using System.Diagnostics;
using System.IO;
using Kettu;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LBPUnion.ProjectLighthouse {
    public class Startup {
        public Startup(IConfiguration configuration) {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddControllers();
            services.AddMvc(options =>
                options.OutputFormatters.Add(new XmlOutputFormatter()));
            
            services.AddDbContext<Database>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if(env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            // Logs every request and the response to it
            // Example: "200, 13ms: GET /LITTLEBIGPLANETPS3_XML/news"
            // Example: "404, 127ms: GET /asdasd?query=osucookiezi727ppbluezenithtopplayhdhr"
            app.Use(async (context, next) => {
                Stopwatch requestStopwatch = new();
                requestStopwatch.Start();
                
                context.Request.EnableBuffering(); // Allows us to reset the position of Request.Body for later logging
                await next(); // Handle the request so we can get the status code from it
                
                requestStopwatch.Stop();
                
                Logger.Log(
                    $"{context.Response.StatusCode}, {requestStopwatch.ElapsedMilliseconds}ms: {context.Request.Method} {context.Request.Path}{context.Request.QueryString}",
                    LoggerLevelHttp.Instance
                );
                
                if(context.Request.Method == "POST") {
                    context.Request.Body.Position = 0;
                    Logger.Log(await new StreamReader(context.Request.Body).ReadToEndAsync(), LoggerLevelHttp.Instance);
                }
            });

            app.UseRouting();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}