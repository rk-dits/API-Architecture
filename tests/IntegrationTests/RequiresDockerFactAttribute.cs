using System;
using Xunit;

namespace IntegrationTests;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class RequiresDockerFactAttribute : FactAttribute
{
    public RequiresDockerFactAttribute()
    {
        if (!DockerDetector.IsDockerAvailable())
        {
            Skip = "Docker not available; skipping integration test.";
        }
    }
}