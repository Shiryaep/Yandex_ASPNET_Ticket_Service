using Yandex_ASPNET_Ticket_Service.EventServices;
using Yandex_ASPNET_Ticket_Service.Models;

namespace EventService_Tests;

public class EventServiceTests
{
    private readonly EventService _eventService;

    public EventServiceTests()
    {
        _eventService = new EventService();
    }

    #region SuccessFull scenarious

    [Fact]
    public void AddEvent_ValidEvent_ReturnsCreatedEvent()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var startDate = DateTime.Parse("2026-03-22T10:30:00");
        var stopDate = DateTime.Parse("2026-03-22T12:30:00");
        var @event = new Event
        {
            Id = eventId,
            Title = "Yandex Asp.Net Study",
            Description = "With great power comes great responsibility.",
            StartAt = startDate,
            EndAt = stopDate
        };

        // Act
        var result = _eventService.AddEvent(@event);

        // Assert
        Assert.Equal(@event, result);
        Assert.Contains(@event, _eventService.GetEvents().CurrentEvents);
    }

    [Fact]
    public void GetEvent_ExistingId_ReturnsEventById()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var startDate = DateTime.Parse("2026-03-22T10:30:00");
        var stopDate = DateTime.Parse("2026-03-22T12:30:00");
        var @event = new Event
        {
            Id = eventId,
            Title = "Yandex Asp.Net Study",
            Description = "With great power comes great responsibility.",
            StartAt = startDate,
            EndAt = stopDate
        };
        _eventService.AddEvent(@event);

        // Act
        var result = _eventService.GetEvent(eventId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(eventId, result.Id);
        Assert.Equal("Yandex Asp.Net Study", result.Title);
    }

    [Fact]
    public void GetEvents_NoFilters_ReturnsAllEvents()
    {
        // Arrange
        var startDate = DateTime.Parse("2026-03-22T10:30:00");
        var stopDate = DateTime.Parse("2026-03-22T12:30:00");
        var event1 = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Yandex Asp.Net Study 1",
            StartAt = startDate,
            EndAt = stopDate
        };
        var event2 = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Yandex Asp.Net Study 2",
            StartAt = startDate,
            EndAt = stopDate
        };
        _eventService.AddEvent(event1);
        _eventService.AddEvent(event2);

        // Act
        var result = _eventService.GetEvents();

        // Assert
        Assert.Equal(2, result.TotalEventsCount);
        Assert.Contains(event1, result.CurrentEvents);
        Assert.Contains(event2, result.CurrentEvents);
    }

    [Fact]
    public void GetEvents_FilterByTitle_ReturnsMatchingEvents()
    {
        // Arrange
        var startDate = DateTime.Parse("2026-03-22T10:30:00");
        var stopDate = DateTime.Parse("2026-03-22T12:30:00");
        var event1 = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Yandex Asp.Net Study",
            StartAt = startDate,
            EndAt = stopDate
        };
        var event2 = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Yandex Fullstack Study",
            StartAt = startDate,
            EndAt = stopDate
        };
        _eventService.AddEvent(event1);
        _eventService.AddEvent(event2);

        // Act
        var result = _eventService.GetEvents(title: "Yandex Asp.Net Study");

        // Assert
        Assert.Single(result.CurrentEvents);
        Assert.Equal("Yandex Asp.Net Study", result.CurrentEvents[0].Title);
    }

    [Fact]
    public void GetEvents_FilterByStartDate_ReturnsEventsAfterStart()
    {
        // Arrange
        var event1 = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Yandex Asp.Net Study 1",
            StartAt = DateTime.Parse("2026-03-22T10:30:00"),
            EndAt = DateTime.Parse("2026-03-22T12:30:00")
        };
        var event2 = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Yandex Asp.Net Study 2",
            StartAt = DateTime.Parse("2026-03-30T10:30:00"),
            EndAt = DateTime.Parse("2026-03-30T12:30:00")
        };
        _eventService.AddEvent(event1);
        _eventService.AddEvent(event2);

        // Act
        var result = _eventService.GetEvents(from: DateTime.Parse("2026-03-25T10:30:00"));

        // Assert
        Assert.Single(result.CurrentEvents);
        Assert.Equal("Yandex Asp.Net Study 2", result.CurrentEvents[0].Title);
    }

    [Fact]
    public void GetEvents_FilterByEndDate_ReturnsEventsBeforeEnd()
    {
        // Arrange
        var event1 = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Yandex Asp.Net Study 1",
            StartAt = DateTime.Parse("2026-03-22T10:30:00"),
            EndAt = DateTime.Parse("2026-03-22T12:30:00")
        };
        var event2 = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Yandex Asp.Net Study 2",
            StartAt = DateTime.Parse("2026-03-30T10:30:00"),
            EndAt = DateTime.Parse("2026-03-30T12:30:00")
        };
        _eventService.AddEvent(event1);
        _eventService.AddEvent(event2);

        // Act
        var result = _eventService.GetEvents(to: DateTime.Parse("2026-03-25T10:30:00"));

        // Assert
        Assert.Single(result.CurrentEvents);
        Assert.Equal("Yandex Asp.Net Study 1", result.CurrentEvents[0].Title);
    }

    [Fact]
    public void GetEvents_Pagination_ReturnsCorrectPage()
    {
        // Arrange
        var startDate = DateTime.Parse("2026-03-22T10:30:00");
        var stopDate = DateTime.Parse("2026-03-22T12:30:00");
        for (int i = 1; i <= 25; i++)
        {
            _eventService.AddEvent(new Event
            {
                Id = Guid.NewGuid(),
                Title = $"Yandex Event number {i}",
                StartAt = startDate,
                EndAt = stopDate
            });
        }

        // Act
        var page1 = _eventService.GetEvents(page: 1, pageSize: 10);
        var page3 = _eventService.GetEvents(page: 3, pageSize: 10);

        // Assert
        Assert.Equal(10, page1.CurrentEvents.Count);
        Assert.Equal(5, page3.CurrentEvents.Count);
        Assert.Equal(25, page1.TotalEventsCount);
        Assert.Equal(25, page3.TotalEventsCount);
        Assert.NotEqual(page1.CurrentEvents[0].Id, page3.CurrentEvents[0].Id);
    }

    [Fact]
    public void GetEvents_CombinedFilters_ReturnsFilteredEvents()
    {
        // Arrange
        var event1 = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Yandex Asp.Net Study 1",
            StartAt = DateTime.Parse("2026-03-22T10:30:00"),
            EndAt = DateTime.Parse("2026-03-22T12:30:00")
        };
        var event2 = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Yandex Asp.Net Study 2",
            StartAt = DateTime.Parse("2026-03-30T10:30:00"),
            EndAt = DateTime.Parse("2026-03-30T12:30:00")
        };
        var event3 = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Yandex Asp.Net Study 3",
            StartAt = DateTime.Parse("2026-03-22T10:30:00"),
            EndAt = DateTime.Parse("2026-03-22T12:30:00")
        };
        var event4 = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Yandex Asp.Net With Correct Date",
            StartAt = DateTime.Parse("2026-03-30T10:30:00"),
            EndAt = DateTime.Parse("2026-03-30T12:30:00")
        };
        _eventService.AddEvent(event1);
        _eventService.AddEvent(event2);
        _eventService.AddEvent(event3);

        // Act
        var result = _eventService.GetEvents(title: "Yandex Asp.Net Study", from: DateTime.Parse("2026-03-25T12:30:00"));

        // Assert
        Assert.Single(result.CurrentEvents);
        Assert.Equal("Yandex Asp.Net Study 2", result.CurrentEvents[0].Title);
    }

    [Fact]
    public void UpdateEvent_ExistingId_UpdatesEvent()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var originalEvent = new Event
        {
            Id = eventId,
            Title = "Yandex Asp.Net Study",
            Description = "With great power comes great responsibility.",
            StartAt = DateTime.Parse("2026-03-22T10:30:00"),
            EndAt = DateTime.Parse("2026-03-22T12:30:00")
        };
        _eventService.AddEvent(originalEvent);

        var updatedEvent = new Event
        {
            Id = eventId,
            Title = "Yandex Asp.Net Study UPDATE",
            Description = "With great power comes great responsibility. SPIDER-MAN",
            StartAt = DateTime.Parse("2026-03-23T10:30:00"),
            EndAt = DateTime.Parse("2026-03-23T12:30:00")
        };

        // Act
        _eventService.UpdateEvent(eventId, updatedEvent);
        var result = _eventService.GetEvent(eventId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Yandex Asp.Net Study UPDATE", result.Title);
        Assert.Equal("With great power comes great responsibility. SPIDER-MAN", result.Description);
        Assert.Equal(DateTime.Parse("2026-03-23T10:30:00"), result.StartAt);
    }

    [Fact]
    public void DeleteEvent_ExistingId_RemovesEvent()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var @event = new Event
        {
            Id = eventId,
            Title = "Yandex Asp.Net Study DELETE",
            Description = "With great power comes great responsibility.",
            StartAt = DateTime.Parse("2026-03-22T10:30:00"),
            EndAt = DateTime.Parse("2026-03-22T12:30:00")
        };
        _eventService.AddEvent(@event);

        // Act
        _eventService.DeleteEvent(eventId);
        var result = _eventService.GetEvent(eventId);

        // Assert
        Assert.Null(result);
        Assert.DoesNotContain(@event, _eventService.GetEvents().CurrentEvents);
    }

    [Fact]
    public void GetEvent_NonExistentId_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = _eventService.GetEvent(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void UpdateEvent_NonExistentId_DoesNothing()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var eventToUpdate = new Event
        {
            Id = nonExistentId,
            Title = "Yandex Asp.Net Study UPDATE",
            Description = "With great power comes great responsibility.",
            StartAt = DateTime.Parse("2026-03-22T10:30:00"),
            EndAt = DateTime.Parse("2026-03-22T12:30:00")
        };

        // Act
        _eventService.UpdateEvent(nonExistentId, eventToUpdate);

        // Assert
        var result = _eventService.GetEvent(nonExistentId);
        Assert.Null(result);
    }

    [Fact]
    public void DeleteEvent_NonExistentId_DoesNothing()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var initialCount = _eventService.GetEvents().TotalEventsCount;

        // Act
        _eventService.DeleteEvent(nonExistentId);

        // Assert
        var afterCount = _eventService.GetEvents().TotalEventsCount;
        Assert.Equal(initialCount, afterCount);
    }
    #endregion
}