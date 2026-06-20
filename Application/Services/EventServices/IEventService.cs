using Application.DTO;
using Domain;
using System.Reflection;

namespace Application.Services.EventServices;

/// <summary> Service Interface for events manipulation </summary>
public interface IEventService
{
    /// <summary> Return all created events as list </summary>
    public Task<PaginatedResult<EventInfoDto>> GetAllEventsAsync(string? title = null,
        DateTime? from = null, DateTime? to = null,
        int page = AppConstants.DefaultPage, int pageSize = AppConstants.DefaultPageSize, CancellationToken cancellationToken = default);

    /// <summary> Return event by ID </summary>
    public Task<EventInfoDto> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary> Add new event to events List</summary>
    public Task<EventInfoDto> CreateEventAsync(CreateEventDto createEvent, CancellationToken cancellationToken = default);

    /// <summary> Replace existing event by ID </summary>
    public Task<EventInfoDto> UpdateEventAsync(Guid id, UpdateEventDto updateEvent, CancellationToken cancellationToken = default);

    /// <summary> Remove event from events by ID </summary>
    public Task<bool> DeleteEventAsync(Guid id, CancellationToken cancellationToken = default);
}