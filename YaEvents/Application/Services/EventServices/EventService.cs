using YaContracts;
using YaEvents.Application.DTO;
using YaEvents.Application.Repositories;
using YaEvents.Domain;
using YaEvents.Domain.Exceptions;

namespace YaEvents.Application.Services.EventServices;

/// <summary> Service for events manipulation </summary>
public class EventService(IEventRepository eventRepository,
    ICacheService cache, ICacheHelper cacheHelper) : IEventService
{
    private readonly IEventRepository _eventRepository = eventRepository;
    private readonly ICacheHelper _cacheHelper = cacheHelper;
    private readonly ICacheService _cache = cache;

    /// <summary> Return all created events as list using filters</summary>
    public async Task<PaginatedResult<EventInfoDto>> GetAllEventsAsync(string? title = null,
        DateTime? from = null, DateTime? to = null,
        int page = Constants.DefaultPage,
        int pageSize = Constants.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _eventRepository.GetFilteredQuery(title, from, to);

        var totalCount = await _eventRepository.GetCountOfQuery(query);

        var items = await _eventRepository.GetPaginatedItemsOfQuery(query, page, pageSize);

        return new PaginatedResult<EventInfoDto>
        {
            Items = items.Select(ToInfo).ToArray(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary> Return event by ID </summary>
    public async Task<EventInfoDto> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cachedVal = await _cache.GetAsync<EventInfoDto>(Constants.GetEventByIdCacheKey + id.ToString());
        if (cachedVal != null)
            return cachedVal;

        var @event = await _eventRepository.GetEventByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Event not found");

        var infoEvent = ToInfo(@event);
        await _cacheHelper.UpdateEventInCacheAsync(infoEvent);

        return infoEvent;
    }

    public async Task<List<EventInfoDto>> GetTopEventsAsync(CancellationToken cancellationToken = default)
    {
        var cachedVal = await _cache.GetAsync<List<EventInfoDto>>(Constants.TopEventsCacheKey);
        if (cachedVal != null)
            return cachedVal;

        var topEvents = await _eventRepository.GetTopEventsAsync(cancellationToken);

        var result = topEvents.Select(e => ToInfo(e)).ToList();

        await _cacheHelper.UpdateTopEventsInCacheAsync(result);

        return result;
    }

    /// <summary> Add new event to events List</summary>
    public async Task<EventInfoDto> CreateEventAsync(CreateEventDto createEvent, CancellationToken cancellationToken = default)
    {
        Event @event = Event.Create(createEvent.Title, createEvent.Description, createEvent.StartAt, createEvent.EndAt, createEvent.TotalSeats);
        await _eventRepository.AddEventAsync(@event, cancellationToken);
        await _eventRepository.SaveChangesAsync(cancellationToken);
        return ToInfo(@event);
    }

    /// <summary> Update existing event by ID </summary>
    public async Task<EventInfoDto> UpdateEventAsync(Guid id, UpdateEventDto updateEvent, CancellationToken cancellationToken = default)
    {
        var @event = await _eventRepository.GetEventByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Event not found");
        @event.Update(updateEvent.Title, updateEvent.Description, updateEvent.StartAt, updateEvent.EndAt);
        await _eventRepository.SaveChangesAsync(cancellationToken);

        var infoEvent = ToInfo(@event);
        await _cacheHelper.UpdateEventInCacheAsync(infoEvent);

        return infoEvent;
    }

    public async Task<bool> DeleteEventAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var @event = await _eventRepository.GetEventByIdAsync(id, cancellationToken);
        if (@event == null)
            return false;

        _eventRepository.DeleteEventAsync(@event);
        await _eventRepository.SaveChangesAsync(cancellationToken);
        await _cacheHelper.DeleteEventFromCacheAsync(@event.Id);
        return true;
    }

    public static EventInfoDto ToInfo(Event @event) => new()
    {
        Id = @event.Id,
        Title = @event.Title,
        Description = @event.Description,
        StartAt = @event.StartAt,
        EndAt = @event.EndAt,
        TotalSeats = @event.TotalSeats,
        AvailableSeats = @event.AvailableSeats
    };
}