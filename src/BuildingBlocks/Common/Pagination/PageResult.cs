namespace BuildingBlocks.Common.Pagination;

public sealed record PageResult<T>(IReadOnlyCollection<T> Items, int Total, int Page, int Size)
{
    public int Pages => (int)Math.Ceiling((double)Total / Size);
    public bool HasNext => Page < Pages;
    public bool HasPrevious => Page > 1;
    public static PageResult<T> Empty(int page, int size) => new(Array.Empty<T>(), 0, page, size);
}
