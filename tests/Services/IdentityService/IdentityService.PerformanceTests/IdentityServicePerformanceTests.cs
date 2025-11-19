using NBomber.CSharp;
using NBomber.Http.CSharp;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text;
using Xunit;

namespace IdentityService.PerformanceTests;

public class IdentityServicePerformanceTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly string _baseUrl;

    public IdentityServicePerformanceTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _baseUrl = "https://localhost:7001"; // Adjust as needed
    }

    [Fact]
    public void LoadTest_HealthEndpoint_ShouldHandleLoad()
    {
        var scenario = Scenario.Create("health_check", async context =>
        {
            var response = await HttpClientBuilder
                .Create()
                .WithBaseAddress(_baseUrl)
                .Build()
                .GetAsync("/health");

            return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
        })
        .WithLoadSimulations(
            Simulation.InjectPerSec(rate: 10, during: TimeSpan.FromSeconds(30))
        );

        NBomberRunner
            .RegisterScenarios(scenario)
            .Run();
    }

    [Fact]
    public void LoadTest_UserRegistration_ShouldHandleLoad()
    {
        var scenario = Scenario.Create("user_registration", async context =>
        {
            var registerRequest = new
            {
                Email = $"loadtest_{Guid.NewGuid():N}@example.com",
                Username = $"loadtestuser_{Guid.NewGuid():N}",
                Password = "LoadTest123!",
                FirstName = "Load",
                LastName = "Test"
            };

            var json = JsonConvert.SerializeObject(registerRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var httpClient = HttpClientBuilder
                .Create()
                .WithBaseAddress(_baseUrl)
                .Build();

            var response = await httpClient.PostAsync("/api/users/register", content);

            return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
        })
        .WithLoadSimulations(
            Simulation.InjectPerSec(rate: 5, during: TimeSpan.FromSeconds(30))
        );

        NBomberRunner
            .RegisterScenarios(scenario)
            .Run();
    }

    [Fact]
    public void LoadTest_UserLogin_ShouldHandleLoad()
    {
        // First, create some test users
        var testUsers = CreateTestUsers(10).Result;

        var scenario = Scenario.Create("user_login", async context =>
        {
            // Pick a random test user
            var randomUser = testUsers[Random.Shared.Next(testUsers.Count)];

            var loginRequest = new
            {
                Email = randomUser.Email,
                Password = randomUser.Password,
                RememberMe = false
            };

            var json = JsonConvert.SerializeObject(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var httpClient = HttpClientBuilder
                .Create()
                .WithBaseAddress(_baseUrl)
                .Build();

            var response = await httpClient.PostAsync("/api/users/login", content);

            return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
        })
        .WithLoadSimulations(
            Simulation.InjectPerSec(rate: 8, during: TimeSpan.FromSeconds(30))
        );

        NBomberRunner
            .RegisterScenarios(scenario)
            .Run();
    }

    [Fact]
    public void StressTest_MixedOperations_ShouldMaintainPerformance()
    {
        // Create some test users first
        var testUsers = CreateTestUsers(5).Result;

        var healthCheckScenario = Scenario.Create("health_check_stress", async context =>
        {
            var httpClient = HttpClientBuilder
                .Create()
                .WithBaseAddress(_baseUrl)
                .Build();

            var response = await httpClient.GetAsync("/health");
            return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
        })
        .WithWeight(30)
        .WithLoadSimulations(
            Simulation.InjectPerSec(rate: 15, during: TimeSpan.FromMinutes(2))
        );

        var registrationScenario = Scenario.Create("registration_stress", async context =>
        {
            var registerRequest = new
            {
                Email = $"stress_{context.ScenarioInfo.IterationNumber}_{Guid.NewGuid():N}@example.com",
                Username = $"stressuser_{context.ScenarioInfo.IterationNumber}_{Guid.NewGuid():N}",
                Password = "StressTest123!",
                FirstName = "Stress",
                LastName = "Test"
            };

            var json = JsonConvert.SerializeObject(registerRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var httpClient = HttpClientBuilder
                .Create()
                .WithBaseAddress(_baseUrl)
                .Build();

            var response = await httpClient.PostAsync("/api/users/register", content);
            return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
        })
        .WithWeight(20)
        .WithLoadSimulations(
            Simulation.InjectPerSec(rate: 5, during: TimeSpan.FromMinutes(2))
        );

        var loginScenario = Scenario.Create("login_stress", async context =>
        {
            var randomUser = testUsers[Random.Shared.Next(testUsers.Count)];

            var loginRequest = new
            {
                Email = randomUser.Email,
                Password = randomUser.Password,
                RememberMe = false
            };

            var json = JsonConvert.SerializeObject(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var httpClient = HttpClientBuilder
                .Create()
                .WithBaseAddress(_baseUrl)
                .Build();

            var response = await httpClient.PostAsync("/api/users/login", content);
            return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
        })
        .WithWeight(50)
        .WithLoadSimulations(
            Simulation.InjectPerSec(rate: 10, during: TimeSpan.FromMinutes(2))
        );

        NBomberRunner
            .RegisterScenarios(healthCheckScenario, registrationScenario, loginScenario)
            .Run();
    }

    private async Task<List<TestUser>> CreateTestUsers(int count)
    {
        var users = new List<TestUser>();
        var client = _factory.CreateClient();

        for (int i = 0; i < count; i++)
        {
            var email = $"perftest_{i}_{Guid.NewGuid():N}@example.com";
            var password = "PerfTest123!";

            var registerRequest = new
            {
                Email = email,
                Username = $"perfuser_{i}_{Guid.NewGuid():N}",
                Password = password,
                FirstName = "Perf",
                LastName = $"User{i}"
            };

            var json = JsonConvert.SerializeObject(registerRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/users/register", content);
            if (response.IsSuccessStatusCode)
            {
                users.Add(new TestUser { Email = email, Password = password });
            }
        }

        return users;
    }
}

public class TestUser
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}