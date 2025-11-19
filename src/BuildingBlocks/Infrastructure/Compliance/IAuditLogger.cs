using System.Collections.Generic;

namespace BuildingBlocks.Infrastructure.Compliance;

public interface IAuditLogger
{
    void EntityCreated(string entityName, object redactedEntity, IDictionary<string, object?> metadata);
}
