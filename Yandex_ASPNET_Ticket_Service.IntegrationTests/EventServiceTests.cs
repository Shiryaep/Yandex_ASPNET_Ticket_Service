using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;
using Yandex_ASPNET_Ticket_Service.DataAccess;
using Yandex_ASPNET_Ticket_Service.Models;
using Yandex_ASPNET_Ticket_Service.Repositories;
using Yandex_ASPNET_Ticket_Service.Services.EventServices;

namespace Yandex_ASPNET_Ticket_Service.IntegrationTests;

public class EventServiceTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
    }

    private async Task ResetDatabaseAsync()
    {
        await using var context = CreateContext();
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Events\", \"Bookings\" RESTART IDENTITY CASCADE;");
    }

    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgreSqlContainer.GetConnectionString())
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    private async Task<List<Event>> CreateTestEventsAsync()
    {
        var context = CreateContext();
        var eventRepository = new EventRepository(context);
        var eventService = new EventService(eventRepository);
        var baseDate = DateTime.UtcNow.Date.AddDays(1);
        var events = new List<Event>
        {
            Event.Create("Concert Rock", "Rock concert",
                baseDate.AddDays(0), baseDate.AddDays(1), 100),
            Event.Create("Jazz Festival", "Jazz festival description",
                baseDate.AddDays(1), baseDate.AddDays(2), 200),
            Event.Create("Classical Concert", "Classical music",
                baseDate.AddDays(2), baseDate.AddDays(3), 150),
            Event.Create("Rock Party", "Party with rock music",
                baseDate.AddDays(3), baseDate.AddDays(4), 80),
            Event.Create("Theater Play", "Drama theater",
                baseDate.AddDays(4), baseDate.AddDays(5), 120),
        };

        foreach (var eventEntity in events)
        {
            await eventRepository.AddEventAsync(eventEntity);
        }
        await eventRepository.SaveChangesAsync();

        return events;
    }

    [Fact]
    public async Task GetAllEventsAsync_NoFilters_ReturnsAllEvents()
    {
        // Arrange
        await ResetDatabaseAsync();
        await CreateTestEventsAsync();
        var context = CreateContext();
        var eventRepository = new EventRepository(context);
        var eventService = new EventService(eventRepository);

        // Act
        var result = await eventService.GetAllEventsAsync();

        // Assert
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(5, result.Items.Length);
    }

    [Fact]
    public async Task GetAllEventsAsync_WithTitleFilter_ReturnsFilteredEvents()
    {
        // Arrange
        await ResetDatabaseAsync();
        await CreateTestEventsAsync();
        var context = CreateContext();
        var eventRepository = new EventRepository(context);
        var eventService = new EventService(eventRepository);

        // Act
        var result = await eventService.GetAllEventsAsync(title: "Rock");

        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.Contains(result.Items, e => e.Title!.Contains("Rock"));
        Assert.DoesNotContain(result.Items, e => e.Title!.Contains("Jazz"));
    }

    [Fact]
    public async Task GetAllEventsAsync_WithFromDateFilter_ReturnsEventsAfterDate()
    {
        // Arrange
        await ResetDatabaseAsync();
        var events = await CreateTestEventsAsync();
        var context = CreateContext();
        var eventRepository = new EventRepository(context);
        var eventService = new EventService(eventRepository);
        var baseDate = events[0].StartAt!.Value; // First event start date
        var fromDate = baseDate.AddDays(2); // Day 3 (index 2)

        // Act
        var result = await eventService.GetAllEventsAsync(from: fromDate);

        // Assert
        Assert.Equal(3, result.TotalCount); // Events starting on day 3, 4, 5 (indices 2, 3, 4)
        Assert.All(result.Items, e => Assert.True(e.StartAt >= fromDate));
    }

    [Fact]
    public async Task GetAllEventsAsync_WithToDateFilter_ReturnsEventsBeforeDate()
    {
        // Arrange
        await ResetDatabaseAsync();
        var events = await CreateTestEventsAsync();
        var context = CreateContext();
        var eventRepository = new EventRepository(context);
        var eventService = new EventService(eventRepository);
        var baseDate = events[0].StartAt!.Value;
        var toDate = baseDate.AddDays(2); // Day 3 (index 2)

        // Act
        var result = await eventService.GetAllEventsAsync(to: toDate);

        // Assert
        Assert.Equal(3, result.TotalCount); // Events starting on day 1, 2, 3 (indices 0,1,2)
        Assert.All(result.Items, e => Assert.True(e.StartAt <= toDate));
    }

    [Fact]
    public async Task GetAllEventsAsync_WithFromAndToDateFilter_ReturnsEventsInRange()
    {
        // Arrange
        await ResetDatabaseAsync();
        var events = await CreateTestEventsAsync();
        var context = CreateContext();
        var eventRepository = new EventRepository(context);
        var eventService = new EventService(eventRepository);
        var baseDate = events[0].StartAt!.Value;
        var fromDate = baseDate.AddDays(1); // Day 2 (index 1)
        var toDate = baseDate.AddDays(3);   // Day 4 (index 3)

        // Act
        var result = await eventService.GetAllEventsAsync(from: fromDate, to: toDate);

        // Assert
        Assert.Equal(3, result.TotalCount); // Events starting on day 2, 3, 4 (indices 1,2,3)
        Assert.All(result.Items, e =>
        {
            Assert.True(e.StartAt >= fromDate);
            Assert.True(e.StartAt <= toDate);
        });
    }

    [Fact]
    public async Task GetAllEventsAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        await ResetDatabaseAsync();
        await CreateTestEventsAsync();
        var context = CreateContext();
        var eventRepository = new EventRepository(context);
        var eventService = new EventService(eventRepository);
        int page = 2;
        int pageSize = 2;

        // Act
        var result = await eventService.GetAllEventsAsync(page: page, pageSize: pageSize);

        // Assert
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(page, result.Page);
        Assert.Equal(pageSize, result.PageSize);
        Assert.Equal(2, result.Items.Length);
    }

    [Fact]
    public async Task GetAllEventsAsync_WithPaginationAndFilters_ReturnsCorrectResults()
    {
        // Arrange
        await ResetDatabaseAsync();
        await CreateTestEventsAsync();
        var context = CreateContext();
        var eventRepository = new EventRepository(context);
        var eventService = new EventService(eventRepository);
        int page = 1;
        int pageSize = 1;

        // Act
        var result = await eventService.GetAllEventsAsync(
            title: "Concert",
            page: page,
            pageSize: pageSize);

        // Assert
        Assert.Equal(2, result.TotalCount); // "Concert Rock" and "Classical Concert"
        Assert.Single(result.Items);
        Assert.Contains("Concert", result.Items[0].Title);
    }

    [Fact]
    public async Task GetAllEventsAsync_WithEmptyResult_ReturnsEmptyPaginatedResult()
    {
        // Arrange
        await ResetDatabaseAsync();
        await CreateTestEventsAsync();
        var context = CreateContext();
        var eventRepository = new EventRepository(context);
        var eventService = new EventService(eventRepository);

        // Act
        var result = await eventService.GetAllEventsAsync(title: "NonExistentEvent");

        // Assert
        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task GetAllEventsAsync_OrderingIsPreserved()
    {
        // Arrange
        await ResetDatabaseAsync();
        var events = await CreateTestEventsAsync();
        var context = CreateContext();
        var eventRepository = new EventRepository(context);
        var eventService = new EventService(eventRepository);
        var expectedOrder = events.OrderBy(e => e.StartAt).ToList();

        // Act
        var result = await eventService.GetAllEventsAsync();

        // Assert
        Assert.Equal(events.Count, result.Items.Length);
    }

    [Fact]
    public async Task GetAllEventsAsync_WithCaseInsensitiveTitleFilter_ReturnsResults()
    {
        // Arrange
        await ResetDatabaseAsync();
        await CreateTestEventsAsync();
        var context = CreateContext();
        var eventRepository = new EventRepository(context);
        var eventService = new EventService(eventRepository);

        // Act
        var resultLower = await eventService.GetAllEventsAsync(title: "rock");
        var resultUpper = await eventService.GetAllEventsAsync(title: "ROCK");

        // Assert
        Assert.Equal(2, resultLower.TotalCount);
        Assert.Equal(2, resultUpper.TotalCount);
    }
}