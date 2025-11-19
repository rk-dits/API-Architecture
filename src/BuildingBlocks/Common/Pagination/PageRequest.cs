namespace BuildingBlocks.Common.Pagination;

public sealed class PageRequest
{
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 20;
    public string? Sort { get; init; }
    public bool Asc { get; init; } = true;

    public int Skip => (Page - 1) * Size;
    public int Take => Size;
}
