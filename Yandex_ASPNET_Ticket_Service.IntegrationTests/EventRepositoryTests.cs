using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;
using Yandex_ASPNET_Ticket_Service.DataAccess;
using Yandex_ASPNET_Ticket_Service.Models;
using Yandex_ASPNET_Ticket_Service.Repositories;

namespace Yandex_ASPNET_Ticket_Service.IntegrationTests;

public class EventRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public EventRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task ResetDatabaseAsync()
    {
        await _fixture.ResetDatabaseAsync();
    }

    private AppDbContext CreateContext()
    {
        return _fixture.CreateContext();
    }

    [Fact]
    public async Task GetEventByIdAsync_WhenEventExists_ReturnsEvent()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = CreateContext();
        var eventRepository = new EventRepository(context);
        var eventEntity = Event.Create(
            "Test Event",
            "Test Description",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2),
            100);

        await eventRepository.AddEventAsync(eventEntity);
        await eventRepository.SaveChangesAsync();

        // Act
        await using var verificationContext = CreateContext();
        var eventRepositoryVerify = new EventRepository(verificationContext);
        var result = await eventRepositoryVerify.GetEventByIdAsync(eventEntity.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(eventEntity.Id, result.Id);
        Assert.Equal("Test Event", result.Title);
    }

    [Fact]
    public async Task GetEventByIdAsync_WhenEventDoesNotExist_ReturnsNull()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = CreateContext();
        var eventRepository = new EventRepository(context);

        // Act
        var result = await eventRepository.GetEventByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddEventAsync_AddsEventToDatabase()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = CreateContext();
        var eventRepository = new EventRepository(context);
        var eventEntity = Event.Create(
            "New Event",
            "Description",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2),
            50);

        // Act
        await eventRepository.AddEventAsync(eventEntity);
        await eventRepository.SaveChangesAsync();

        // Assert - use a separate context to verify
        await using var verificationContext = CreateContext();
        var savedEvent = await verificationContext.Events.FirstOrDefaultAsync(e => e.Id == eventEntity.Id);
        Assert.NotNull(savedEvent);
        Assert.Equal("New Event", savedEvent.Title);
        Assert.Equal(50, savedEvent.TotalSeats);
        Assert.Equal(50, savedEvent.AvailableSeats);
    }

    [Fact]
    public async Task DeleteEventAsync_RemovesEventFromDatabase()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = CreateContext();
        var eventRepository = new EventRepository(context);
        var bookingRepository = new BookingRepository(context);
        var eventEntity = Event.Create(
            "Event to Delete",
            "Description",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2),
            30);

        await eventRepository.AddEventAsync(eventEntity);
        await eventRepository.SaveChangesAsync();

        // Act
        eventRepository.DeleteEventAsync(eventEntity);
        await eventRepository.SaveChangesAsync();

        // Assert - use a separate context to verify
        await using var verificationContext = CreateContext();
        var deletedEvent = await verificationContext.Events.FindAsync(eventEntity.Id);
        Assert.Null(deletedEvent);
    }

    [Fact]
    public async Task GetEventsAsQuery_ReturnsQueryableEvents()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = CreateContext();
        var eventRepository = new EventRepository(context);
        var event1 = Event.Create("Event 1", null, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 10);
        var event2 = Event.Create("Event 2", null, DateTime.UtcNow.AddDays(3), DateTime.UtcNow.AddDays(4), 20);

        await eventRepository.AddEventAsync(event1);
        await eventRepository.AddEventAsync(event2);
        await eventRepository.SaveChangesAsync();

        // Act
        var query = eventRepository.GetEventsAsQuery();
        var events = query.ToList();

        // Assert
        Assert.Equal(2, events.Count);
        Assert.Contains(events, e => e.Title == "Event 1");
        Assert.Contains(events, e => e.Title == "Event 2");
    }

    [Fact]
    public async Task GetEventsAsQuery_WithFiltering_ReturnsFilteredEvents()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = CreateContext();
        var eventRepository = new EventRepository(context);
        var event1 = Event.Create("Concert", null, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 10);
        var event2 = Event.Create("Theater", null, DateTime.UtcNow.AddDays(3), DateTime.UtcNow.AddDays(4), 20);

        await eventRepository.AddEventAsync(event1);
        await eventRepository.AddEventAsync(event2);
        await eventRepository.SaveChangesAsync();

        // Act
        var query = eventRepository.GetEventsAsQuery();
        var concertEvents = query.Where(e => e.Title.Contains("Concert")).ToList();

        // Assert
        Assert.Single(concertEvents);
        Assert.Equal("Concert", concertEvents[0].Title);
    }

    [Fact]
    public async Task GetEventsAsQuery_WithPagination_ReturnsPaginatedResults()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = CreateContext();
        var eventRepository = new EventRepository(context);
        for (int i = 1; i <= 5; i++)
        {
            var eventEntity = Event.Create($"Event {i}", null,
                DateTime.UtcNow.AddDays(i),
                DateTime.UtcNow.AddDays(i + 1),
                i * 10);
            await eventRepository.AddEventAsync(eventEntity);
        }
        await eventRepository.SaveChangesAsync();

        // Act
        var query = eventRepository.GetEventsAsQuery();
        var page1 = query.OrderBy(e => e.Title).Skip(0).Take(2).ToList();
        var page2 = query.OrderBy(e => e.Title).Skip(2).Take(2).ToList();

        // Assert
        Assert.Equal(2, page1.Count);
        Assert.Equal(2, page2.Count);
        Assert.Equal("Event 1", page1[0].Title);
        Assert.Equal("Event 2", page1[1].Title);
        Assert.Equal("Event 3", page2[0].Title);
        Assert.Equal("Event 4", page2[1].Title);
    }

    [Fact]
    public async Task SaveChangesAsync_PersistsChanges()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = CreateContext();
        var eventRepository = new EventRepository(context);
        var eventEntity = Event.Create("Test", null, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 10);
        await eventRepository.AddEventAsync(eventEntity);

        // Act
        await eventRepository.SaveChangesAsync();

        // Assert - use a separate context to verify
        await using var verificationContext = CreateContext();
        var eventRepositoryVerify = new EventRepository(verificationContext);
        var saved = await eventRepositoryVerify.GetEventByIdAsync(eventEntity.Id);
        Assert.NotNull(saved);
    }
}