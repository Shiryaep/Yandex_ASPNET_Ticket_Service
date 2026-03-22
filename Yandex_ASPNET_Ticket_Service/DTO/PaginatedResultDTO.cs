using Yandex_ASPNET_Ticket_Service.Models;

/// <summary> Paginated result wrapper </summary>
public class PaginatedResult<Event>
{
    /// <summary> Total number of events </summary>
    public int TotalEventsCount { get; set; }

    /// <summary> Events for current page </summary>
    public List<Event> CurrentEvents { get; set; } = [];

    /// <summary> Current page number </summary>
    public int Page { get; set; }

    /// <summary> Number of items per page </summary>
    public int PageSize { get; set; }

    /// <summary> Paginated result wrapper constructor </summary>
    public PaginatedResult()
    {
    }

    /// <summary> Paginated result wrapper constructor with arguments</summary>
    public PaginatedResult(List<Event> eventsList, int totalEventsCount, int page, int pageSize)
    {
        CurrentEvents = eventsList;
        TotalEventsCount = totalEventsCount;
        Page = page;
        PageSize = pageSize;
    }
}