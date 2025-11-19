using System;

namespace BuildingBlocks.Infrastructure.Compliance;

public interface IRedactionService
{
    object Redact(object instance);
}
