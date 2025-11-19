namespace BuildingBlocks.Common.Abstractions;

public sealed record Error(string Code, string? Message = null)
{
    public static readonly Error None = new("None", string.Empty);
    public static Error NotFound(string entity, string id) => new("NotFound", $"{entity} with id '{id}' was not found.");
    public static Error Validation(string message) => new("Validation", message);
    public static Error Conflict(string message) => new("Conflict", message);
    public static Error Unauthorized(string message = "Unauthorized") => new("Unauthorized", message);
    public static Error Forbidden(string message = "Forbidden") => new("Forbidden", message);
}
