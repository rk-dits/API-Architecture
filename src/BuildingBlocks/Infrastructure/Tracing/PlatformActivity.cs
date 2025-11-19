using System.Diagnostics;

namespace BuildingBlocks.Infrastructure.Tracing;

public static class PlatformActivity
{
    public const string SourceName = "Platform";
    public static readonly ActivitySource Source = new(SourceName);
}
