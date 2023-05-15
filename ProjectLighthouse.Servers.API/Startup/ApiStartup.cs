using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Middlewares;
using LBPUnion.ProjectLighthouse.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace LBPUnion.ProjectLighthouse.Servers.API.Startup;

public class ApiStartup
{
    public ApiStartup(IConfiguration configuration)
    {
        this.Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        services.AddMvc
        (
            options =>
            {
                options.OutputFormatters.Add(new JsonOutputFormatter());
            }
        );

        services.AddDbContext<DatabaseContext>(builder =>
        {
            builder.UseMySql(ServerConfiguration.Instance.DbConnectionString,
                MySqlServerVersion.LatestSupportedServerVersion);
        });

        services.AddSwaggerGen
        (
            c =>
            {
                // Give swagger the name and version of our project
                c.SwaggerDoc
                (
                    "v1",
                    new OpenApiInfo
                    {
                        Title = "Project Lighthouse API",
                        Version = "v1",
                    }
                );

                // Filter out endpoints not in /api/v1
                c.DocumentFilter<SwaggerFilter>();

                // Add XMLDoc to swagger
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "LBPUnion.ProjectLighthouse.Servers.API.xml"));
            }
        );
    }

    public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        #if DEBUG
        app.UseDeveloperExceptionPage();
        #endif

        app.UseSwagger();
        app.UseSwaggerUI
        (
            c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "Project Lighthouse API");
            }
        );

        app.UseMiddleware<RequestLogMiddleware>();

        app.UseRouting();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}