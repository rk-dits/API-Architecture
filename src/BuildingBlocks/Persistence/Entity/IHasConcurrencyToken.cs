namespace BuildingBlocks.Persistence.Entity;

public interface IHasConcurrencyToken
{
    string ConcurrencyToken { get; }
}
