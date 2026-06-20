namespace Application.DTO;

/// <summary> Paginated result wrapper </summary>
public class PaginatedResult<T>
{
    /// <summary> Total number of events </summary>
    public required int TotalCount { get; set; }

    /// <summary> Events for current page </summary>
    public required T[] Items { get; set; }

    /// <summary> Current page number </summary>
    public required int Page { get; set; }

    /// <summary> Number of items per page </summary>
    public required int PageSize { get; set; }

    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
}