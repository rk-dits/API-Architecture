namespace BuildingBlocks.Persistence.Entity;

public interface ISoftDelete
{
    bool IsDeleted { get; }
    DateTimeOffset? DeletedAt { get; }
}
