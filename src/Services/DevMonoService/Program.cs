using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;

namespace DevMonoService;

public static class Program
{
    public static void Main(string[] args)
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            Log.Information("Starting Development Mono Service");
            CreateHostBuilder(args).Build().Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Development Mono Service terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
                webBuilder.UseUrls("https://localhost:7000", "http://localhost:5000");
            });
}

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        // Add CORS for development
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });

        services.AddControllers()
            .AddNewtonsoftJson();

        // Configure Swagger for all services
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Development Mono Service - All APIs",
                Version = "v1",
                Description = "Unified API documentation for all services in development environment",
                Contact = new OpenApiContact
                {
                    Name = "Development Team",
                    Email = "dev@company.com"
                }
            });

            // Add JWT Bearer authentication
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header
                    },
                    new List<string>()
                }
            });

            // Include XML comments for better documentation
            var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml");
            foreach (var xmlFile in xmlFiles)
            {
                c.IncludeXmlComments(xmlFile);
            }

            // Custom operation tags for better organization
            c.TagActionsBy(api =>
            {
                var controllerName = api.ActionDescriptor.RouteValues["controller"];
                var serviceName = GetServiceNameFromController(controllerName);
                return new[] { $"{serviceName} - {controllerName}" };
            });
        });

        // Add services from all referenced projects (simplified for development)
        // Note: In production, these would be separate services with proper DI configuration

        // Add minimal required services for development
        services.AddHttpContextAccessor();
        services.AddMemoryCache();
        services.AddHttpClient();

        // Add authentication and authorization (simplified for development)
        services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                // Development-only settings - DO NOT use in production
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = false,
                    RequireExpirationTime = false,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();

            // Enable Swagger for development
            app.UseSwagger(c =>
            {
                c.RouteTemplate = "swagger/{documentName}/swagger.json";
            });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Development Mono Service v1");
                c.RoutePrefix = "swagger";
                c.DisplayRequestDuration();
                c.EnableDeepLinking();
                c.EnableFilter();
                c.ShowExtensions();
                c.EnableValidator();
                c.SupportedSubmitMethods(new[] {
                    Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Get,
                    Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Post,
                    Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Put,
                    Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Delete,
                    Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Patch
                });
            });
        }

        app.UseRouting();
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();

            // Add a health check endpoint
            endpoints.MapGet("/health", async context =>
            {
                await context.Response.WriteAsync("Development Mono Service is running");
            });

            // Redirect root to Swagger
            endpoints.MapGet("/", async context =>
            {
                context.Response.Redirect("/swagger");
            });
        });
    }

    private static string GetServiceNameFromController(string? controllerName)
    {
        return controllerName switch
        {
            var name when name?.Contains("Auth") == true || name?.Contains("Identity") == true => "Identity Service",
            var name when name?.Contains("Integration") == true || name?.Contains("Hub") == true => "Integration Hub",
            var name when name?.Contains("Workflow") == true || name?.Contains("Core") == true => "Core Workflow Service",
            _ => "Unknown Service"
        };
    }
}