using System;
using System.IO;

namespace IntegrationTests;

public static class DockerDetector
{
    public static bool IsDockerAvailable()
    {
        try
        {
            // Common Windows Docker Desktop path
            var dockerExe = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Docker", "Docker", "resources", "bin", "docker.exe");
            if (File.Exists(dockerExe)) return true;
            // Fallback: check DOCKER_HOST env var presence
            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DOCKER_HOST"))) return true;
        }
        catch { /* swallow */ }
        return false;
    }
}