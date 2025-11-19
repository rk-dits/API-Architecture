using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace IdentityService.SecurityTests;

public class IdentityServiceSecurityTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public IdentityServiceSecurityTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Authorization_WithoutToken_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Authorization_WithInvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");

        // Act
        var response = await _client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Authorization_WithExpiredToken_ShouldReturnUnauthorized()
    {
        // Arrange - Create an expired token
        var expiredToken = CreateExpiredJwtToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", expiredToken);

        // Act
        var response = await _client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    public async Task Registration_WithInvalidEmail_ShouldReturnBadRequest(string invalidEmail)
    {
        // Arrange
        var registerRequest = new
        {
            Email = invalidEmail,
            Username = "testuser",
            Password = "ValidPassword123!",
            FirstName = "Test",
            LastName = "User"
        };

        var json = JsonConvert.SerializeObject(registerRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/users/register", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("weak")]
    [InlineData("12345678")]
    [InlineData("password")]
    [InlineData("PASSWORD")]
    [InlineData("Password")]
    [InlineData("Pass123")]
    public async Task Registration_WithWeakPassword_ShouldReturnBadRequest(string weakPassword)
    {
        // Arrange
        var registerRequest = new
        {
            Email = $"test_{Guid.NewGuid():N}@example.com",
            Username = $"testuser_{Guid.NewGuid():N}",
            Password = weakPassword,
            FirstName = "Test",
            LastName = "User"
        };

        var json = JsonConvert.SerializeObject(registerRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/users/register", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithMultipleFailedAttempts_ShouldImplementAccountLockout()
    {
        // Arrange
        var email = $"lockout_test_{Guid.NewGuid():N}@example.com";

        // First register a user
        await RegisterUser(email, "ValidPassword123!");

        var invalidLoginRequest = new
        {
            Email = email,
            Password = "WrongPassword",
            RememberMe = false
        };

        var json = JsonConvert.SerializeObject(invalidLoginRequest);

        // Act - Multiple failed login attempts
        for (int i = 0; i < 6; i++) // Assuming lockout after 5 failed attempts
        {
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/users/login", content);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // Try with correct password - should still be locked
        var correctLoginRequest = new
        {
            Email = email,
            Password = "ValidPassword123!",
            RememberMe = false
        };

        var correctJson = JsonConvert.SerializeObject(correctLoginRequest);
        var correctContent = new StringContent(correctJson, Encoding.UTF8, "application/json");
        var finalResponse = await _client.PostAsync("/api/users/login", correctContent);

        // Assert
        finalResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var responseContent = await finalResponse.Content.ReadAsStringAsync();
        responseContent.Should().Contain("locked", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SqlInjection_InLoginEndpoint_ShouldNotSucceed()
    {
        // Arrange - SQL injection attempts
        var sqlInjectionAttempts = new[]
        {
            "'; DROP TABLE Users; --",
            "' OR '1'='1",
            "admin'--",
            "' OR 1=1--",
            "'; UPDATE Users SET Password='hacked'--"
        };

        foreach (var injectionAttempt in sqlInjectionAttempts)
        {
            var loginRequest = new
            {
                Email = injectionAttempt,
                Password = injectionAttempt,
                RememberMe = false
            };

            var json = JsonConvert.SerializeObject(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/users/login", content);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
            response.StatusCode.Should().NotBe(HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task XssAttempt_InRegistration_ShouldBeSanitized()
    {
        // Arrange
        var xssPayloads = new[]
        {
            "<script>alert('xss')</script>",
            "javascript:alert('xss')",
            "<img src=x onerror=alert('xss')>",
            "';alert('xss');//"
        };

        foreach (var payload in xssPayloads)
        {
            var registerRequest = new
            {
                Email = $"test_{Guid.NewGuid():N}@example.com",
                Username = $"testuser_{Guid.NewGuid():N}",
                Password = "ValidPassword123!",
                FirstName = payload,
                LastName = "User"
            };

            var json = JsonConvert.SerializeObject(registerRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/users/register", content);

            // Assert - Should either reject the input or sanitize it
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<dynamic>(responseContent);

                // Verify that dangerous scripts are not in the response
                var firstName = (string)user?.firstName;
                firstName.Should().NotContain("<script>");
                firstName.Should().NotContain("javascript:");
                firstName.Should().NotContain("onerror");
            }
            else
            {
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }
    }

    [Fact]
    public async Task JwtToken_ShouldContainSecurityClaims()
    {
        // Arrange
        var email = $"jwt_test_{Guid.NewGuid():N}@example.com";
        var password = "ValidPassword123!";

        // Register and login user
        await RegisterUser(email, password);
        var token = await LoginUser(email, password);

        // Act
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);

        // Assert
        jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Email);
        jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier);
        jsonToken.Claims.Should().Contain(c => c.Type == "jti"); // JWT ID
        jsonToken.ValidTo.Should().BeAfter(DateTime.UtcNow); // Token should not be expired
        jsonToken.ValidFrom.Should().BeBefore(DateTime.UtcNow); // Token should be valid from past
    }

    [Fact]
    public async Task RateLimiting_ShouldPreventBruteForceAttacks()
    {
        // Arrange
        var email = $"ratelimit_test_{Guid.NewGuid():N}@example.com";
        await RegisterUser(email, "ValidPassword123!");

        var loginRequest = new
        {
            Email = email,
            Password = "WrongPassword",
            RememberMe = false
        };

        var json = JsonConvert.SerializeObject(loginRequest);

        // Act - Make many rapid requests
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 20; i++)
        {
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            tasks.Add(_client.PostAsync("/api/users/login", content));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert - Should have some rate limited responses
        var rateLimitedCount = responses.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests);
        rateLimitedCount.Should().BeGreaterThan(0, "Rate limiting should prevent too many rapid requests");
    }

    [Fact]
    public async Task CORS_Headers_ShouldBeProperlyConfigured()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.Headers.Should().ContainKey("Access-Control-Allow-Origin");

        // Check for security headers
        response.Headers.Should().ContainKey("X-Content-Type-Options");
        response.Headers.Should().ContainKey("X-Frame-Options");
        response.Headers.Should().ContainKey("X-XSS-Protection");
    }

    private async Task RegisterUser(string email, string password)
    {
        var registerRequest = new
        {
            Email = email,
            Username = $"user_{Guid.NewGuid():N}",
            Password = password,
            FirstName = "Test",
            LastName = "User"
        };

        var json = JsonConvert.SerializeObject(registerRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/users/register", content);
        response.EnsureSuccessStatusCode();
    }

    private async Task<string> LoginUser(string email, string password)
    {
        var loginRequest = new
        {
            Email = email,
            Password = password,
            RememberMe = false
        };

        var json = JsonConvert.SerializeObject(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/users/login", content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var loginResult = JsonConvert.DeserializeObject<dynamic>(responseContent);

        return (string)loginResult!.accessToken;
    }

    private string CreateExpiredJwtToken()
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes("ThisIsAVeryLongSecretKeyForTestingPurposesOnly123456789");
        var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim("sub", "test-user-id") }),
            Expires = DateTime.UtcNow.AddMinutes(-10), // Expired 10 minutes ago
            SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}