using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using ProjectLighthouse.Serialization;

namespace ProjectLighthouse {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {

            services.AddControllers();
            services.AddMvc(options =>
                options.OutputFormatters.Add(new XmlOutputFormatter()));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if(env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            // Logs every request and the response to it
            // Example: "200: GET /LITTLEBIGPLANETPS3_XML/news"
            // Example: "404: GET /asdasd?query=osucookiezi727ppbluezenithtopplayhdhr"
            app.Use(async (context, next) => {
                context.Request.EnableBuffering(); // Allows us to reset the position of Request.Body for later logging
                await next(); // Handle the request so we can get the status code from it
                Console.WriteLine($"{context.Response.StatusCode}: {context.Request.Method} {context.Request.Path}{context.Request.QueryString}");
                if(context.Request.Method == "POST") {
                    context.Request.Body.Position = 0;
                    Console.WriteLine(await new StreamReader(context.Request.Body).ReadToEndAsync());
                }
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}