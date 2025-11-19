using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace IntegrationTests;

public class IntegrationHubApiTests
{
    [Fact(Skip = "Docker & database not available; integration test deferred.")]
    public void DeferredUntilDockerAvailable() { }
}