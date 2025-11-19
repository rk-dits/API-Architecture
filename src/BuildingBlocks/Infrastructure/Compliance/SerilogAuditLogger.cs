using System.Collections.Generic;
using Serilog;

namespace BuildingBlocks.Infrastructure.Compliance;

public class SerilogAuditLogger : IAuditLogger
{
    private readonly ILogger _logger;
    public SerilogAuditLogger()
    {
        _logger = Log.ForContext<SerilogAuditLogger>();
    }

    public void EntityCreated(string entityName, object redactedEntity, IDictionary<string, object?> metadata)
    {
        _logger.Information("AUDIT EntityCreated {Entity} {@Redacted} {@Metadata}", entityName, redactedEntity, metadata);
    }
}
