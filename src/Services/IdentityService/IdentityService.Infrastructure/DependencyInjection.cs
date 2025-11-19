using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;
using IdentityService.Domain.Services;
using IdentityService.Infrastructure.Data;
using IdentityService.Infrastructure.Repositories;
using IdentityService.Infrastructure.Services;

namespace IdentityService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext - Use InMemory for development if no SQL Server available
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var useInMemory = configuration.GetValue<bool>("UseInMemoryDatabase", false);

        services.AddDbContext<IdentityDbContext>(options =>
        {
            if (useInMemory)
            {
                options.UseInMemoryDatabase("IdentityServiceDb");
            }
            else
            {
                options.UseSqlServer(connectionString);
            }
        });

        // Add Identity
        services.AddIdentity<User, Role>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false;
        })
        .AddEntityFrameworkStores<IdentityDbContext>()
        .AddDefaultTokenProviders();

        // Add JWT Authentication
        var jwtKey = configuration["Jwt:SecretKey"] ?? "your-256-bit-secret-key-here-make-it-long-enough";
        var key = Encoding.UTF8.GetBytes(jwtKey);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        // Register available repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IUserSessionRepository, UserSessionRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IApiKeyRepository, ApiKeyRepository>();

        // TODO: Register domain services when implementation files are available
        // services.AddScoped<IAuthorizationService, AuthorizationService>();
        // services.AddScoped<IMfaService, MfaService>();
        // services.AddScoped<IPasswordService, PasswordService>();
        // services.AddScoped<ISessionService, SessionService>();
        // services.AddScoped<IAuditService, AuditService>();
        // services.AddScoped<IOrganizationService, OrganizationService>();
        // services.AddScoped<INotificationService, NotificationService>();

        // Add ASP.NET Core Identity dependencies
        services.AddScoped<IPasswordHasher<object>, PasswordHasher<object>>();

        // Add services
        // Register working services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IAuthorizationService, AuthorizationService>();
        services.AddScoped<IMfaService, MfaService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<INotificationService, NotificationService>();

        // TODO: Register additional services when implementation files are available
        // services.AddScoped<ISsoService, SsoService>();

        return services;
    }
}