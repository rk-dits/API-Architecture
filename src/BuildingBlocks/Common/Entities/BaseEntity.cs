namespace Common.Entities;

public abstract class Entity<TKey> where TKey : struct
{
    public TKey Id { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TKey> other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return Id.Equals(other.Id);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}

public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    string? CreatedBy { get; set; }
    DateTime? UpdatedAt { get; set; }
    string? UpdatedBy { get; set; }
}

public interface IAggregateRoot
{
    // Marker interface for aggregate roots
}

public abstract class AggregateRoot<TKey> : Entity<TKey>, IAggregateRoot where TKey : struct
{
}

public abstract class AuditableEntity<TKey> : Entity<TKey>, IAuditableEntity where TKey : struct
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}