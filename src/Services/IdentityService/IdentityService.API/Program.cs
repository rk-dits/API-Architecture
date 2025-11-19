using IdentityService.Application;
using IdentityService.Infrastructure;
using IdentityService.Presentation;
using Serilog;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add Swagger/OpenAPI with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Identity Service API",
        Version = "v1",
        Description = "Enterprise Identity & Authentication Service",
        Contact = new OpenApiContact
        {
            Name = "API Support",
            Email = "support@company.com"
        }
    });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
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
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add application layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPresentation();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity Service API V1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Ensure database is created
try
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<IdentityService.Infrastructure.Data.IdentityDbContext>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var useInMemory = configuration.GetValue<bool>("UseInMemoryDatabase", false);

    Log.Information("Database configuration - UseInMemoryDatabase: {UseInMemory}", useInMemory);
    Log.Information("Connection String: {ConnectionString}", configuration.GetConnectionString("DefaultConnection"));

    if (useInMemory)
    {
        // For InMemory database, just ensure the context is available
        Log.Information("Using InMemory database - initializing context");
        try
        {
            await context.Database.EnsureCreatedAsync();
            Log.Information("InMemory database context initialized successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to initialize InMemory database context");
            throw;
        }
    }
    else
    {
        // Only try to create database if using SQL Server
        Log.Information("Using SQL Server database - creating database");
        await context.Database.EnsureCreatedAsync();
    }

    Log.Information("Database initialized successfully");
}
catch (Exception ex)
{
    Log.Fatal(ex, "An error occurred while initializing the database");
    throw;
}

Log.Information("Identity Service starting up...");

app.Run();

// Make the implicit Program class public so test projects can access it
public partial class Program { }