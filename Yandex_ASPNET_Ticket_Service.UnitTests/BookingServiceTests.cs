using Application.DTO;
using Application.Repositories;
using Application.Services.BookingServices;
using Application.Services.EventServices;
using Application.Services.UserServices;
using Domain;
using Domain.Exceptions;
using Infrastructure.DataAccess;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Yandex_ASPNET_Ticket_Service.UnitTests;

public sealed class BookingServiceTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IServiceScope _scope;
    private readonly IEventService _eventService;
    private readonly IBookingService _bookingService;
    private readonly IUserService _userService;
    private readonly IBookingRepository _bookingRepository;

    public BookingServiceTests()
    {
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName));
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IUserService, UserService>();

        _serviceProvider = services.BuildServiceProvider();
        _scope = _serviceProvider.CreateScope();
        _eventService = _scope.ServiceProvider.GetRequiredService<IEventService>();
        _bookingService = _scope.ServiceProvider.GetRequiredService<IBookingService>();
        _userService = _scope.ServiceProvider.GetRequiredService<IUserService>();
        _bookingRepository = _scope.ServiceProvider.GetRequiredService<IBookingRepository>();
    }

    public void Dispose()
    {
        _scope.Dispose();
        _serviceProvider.Dispose();
    }

    private async Task<Guid> CreateTestEventAsync(int totalSeats = 10)
    {
        var futureDate = DateTime.UtcNow.AddDays(1);
        var created = await _eventService.CreateEventAsync(new CreateEventDto
        {
            Title = "Test Event",
            StartAt = futureDate,
            EndAt = futureDate.AddHours(2),
            TotalSeats = totalSeats
        });
        return created.Id;
    }

    private async Task<UserInfoDto> CreateTestUserAsync(string login = "Login", UserRoles role = UserRoles.User)
    {
        var testUser = await _userService.RegisterUserAsync(login, "Password", role);
        return testUser;
    }

    #region CreateBookingAsync Tests

    [Fact]
    public async Task CreateBookingAsync_WithValidEventId_ReturnsBookingInfoWithPendingStatus()
    {
        var eventId = await CreateTestEventAsync();
        var testUser = await CreateTestUserAsync();
        var result = await _bookingService.CreateBookingAsync(eventId, testUser.Id);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(eventId, result.EventId);
        Assert.Equal(BookingStatus.Pending, result.Status);
        Assert.Null(result.ProcessedAt);
        Assert.True(result.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public async Task CreateBookingAsync_WithValidEventId_SetsCreatedAt()
    {
        var eventId = await CreateTestEventAsync();
        var testUser = await CreateTestUserAsync();
        var before = DateTime.UtcNow;

        var result = await _bookingService.CreateBookingAsync(eventId, testUser.Id);

        var after = DateTime.UtcNow;
        Assert.InRange(result.CreatedAt, before, after);
    }

    [Fact]
    public async Task CreateBookingAsync_WithNonExistentEvent_ThrowsNotFoundException()
    {
        var invalidEventId = Guid.NewGuid();
        var testUser = await CreateTestUserAsync();
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _bookingService.CreateBookingAsync(invalidEventId, testUser.Id));
        Assert.Equal("Event not found", exception.Message);
    }

    [Fact]
    public async Task CreateBookingAsync_MultipleBookingsForSameEvent_AllCreatedWithUniqueIds()
    {
        var eventId = await CreateTestEventAsync(totalSeats: 5);
        var testUser = await CreateTestUserAsync();

        var results = new List<BookingInfoDto>();
        for (int i = 0; i < 5; i++)
            results.Add(await _bookingService.CreateBookingAsync(eventId, testUser.Id));

        var uniqueIds = results.Select(r => r.Id).Distinct();
        Assert.Equal(5, uniqueIds.Count());
    }

    [Fact]
    public async Task CreateBookingAsync_WhenNoSeatsAvailable_ThrowsNoAvailableSeatsException()
    {
        var eventId = await CreateTestEventAsync(totalSeats: 1);
        var testUser = await CreateTestUserAsync();
        await _bookingService.CreateBookingAsync(eventId, testUser.Id);

        await Assert.ThrowsAsync<NoAvailableSeatsException>(
            () => _bookingService.CreateBookingAsync(eventId, testUser.Id));
    }

    [Fact]
    public async Task CreateBookingAsync_DecrementsAvailableSeats()
    {
        var eventId = await CreateTestEventAsync(totalSeats: 3);
        var testUser = await CreateTestUserAsync();

        await _bookingService.CreateBookingAsync(eventId, testUser.Id);
        await _bookingService.CreateBookingAsync(eventId, testUser.Id);

        var eventInfo = await _eventService.GetEventByIdAsync(eventId);
        Assert.Equal(1, eventInfo.AvailableSeats);
    }

    [Fact]
    public async Task CreateBookingAsync_EventInThePast_ThrowsAlreadyEndedEventException()
    {
        var pastDate = DateTime.UtcNow.AddMilliseconds(150);
        var created = await _eventService.CreateEventAsync(new CreateEventDto
        {
            Title = "Test Event",
            StartAt = pastDate,
            EndAt = pastDate.AddSeconds(2),
            TotalSeats = 10
        });
        var testUser = await CreateTestUserAsync();
        Thread.Sleep(300);

        await Assert.ThrowsAsync<AlreadyEndedEventException>(
            () => _bookingService.CreateBookingAsync(created.Id, testUser.Id));
    }

    [Fact]
    public async Task CreateBookingAsync_TooManyBookingsOnOneUser_ThrowsBookingLimitExceededException()
    {
        var eventId1 = await CreateTestEventAsync(totalSeats: 30);
        var eventId2 = await CreateTestEventAsync(totalSeats: 30);
        var testUser = await CreateTestUserAsync();

        for (byte i = 0; i < 10; i++)
        {
            await _bookingService.CreateBookingAsync(eventId1, testUser.Id);
        }

        // Throws when booking THE SAME event
        await Assert.ThrowsAsync<BookingLimitExceededException>(
            () => _bookingService.CreateBookingAsync(eventId1, testUser.Id));

        // BUT it is possible to book ANOTHER event
        var bookingInfo = await _bookingService.CreateBookingAsync(eventId2, testUser.Id);
        Assert.Equal(eventId2, bookingInfo.EventId);
        Assert.Equal(testUser.Id, bookingInfo.UserId);
    }

    [Fact]
    public async Task CreateBookingAsync_TooManyBookingsOnOneUserNotAffectAnother_UserTwoCreateBookingSuccessfully()
    {
        var eventId = await CreateTestEventAsync(totalSeats: 30);
        var testUser1 = await CreateTestUserAsync(login: "Login 1");
        var testUser2 = await CreateTestUserAsync(login: "Login 2");

        for (byte i = 0; i < 10; i++)
        {
            await _bookingService.CreateBookingAsync(eventId, testUser1.Id);
        }

        // Throws when user ONE booking event
        await Assert.ThrowsAsync<BookingLimitExceededException>(
            () => _bookingService.CreateBookingAsync(eventId, testUser1.Id));

        // BUT it is possible to book event with user TWO
        var bookingInfo = await _bookingService.CreateBookingAsync(eventId, testUser2.Id);
        Assert.Equal(eventId, bookingInfo.EventId);
        Assert.Equal(testUser2.Id, bookingInfo.UserId);
    }

    #endregion

    #region GetBookingByIdAsync Tests

    [Fact]
    public async Task GetBookingByIdAsync_WithValidId_ReturnsCorrectBookingInfo()
    {
        var eventId = await CreateTestEventAsync();
        var testUser = await CreateTestUserAsync();
        var created = await _bookingService.CreateBookingAsync(eventId, testUser.Id);

        var result = await _bookingService.GetBookingByIdAsync(created.Id);

        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal(created.EventId, result.EventId);
        Assert.Equal(BookingStatus.Pending, result.Status);
    }

    [Fact]
    public async Task GetBookingByIdAsync_WithNonExistentId_ThrowsNotFoundException()
    {
        var invalidId = Guid.NewGuid();

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _bookingService.GetBookingByIdAsync(invalidId));
        Assert.Equal("Booking not found", exception.Message);
    }

    #endregion

    #region CancelBookingByIdAsync Tests

    [Fact]
    public async Task CancelBookingByIdAsync_BookingUserEventNotFound_ThrowsNotFoundException()
    {
        var eventId = await CreateTestEventAsync();
        var testUser = await CreateTestUserAsync();
        var result = await _bookingService.CreateBookingAsync(eventId, testUser.Id);

        //In general booking created
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(eventId, result.EventId);
        Assert.Equal(BookingStatus.Pending, result.Status);
        Assert.Null(result.ProcessedAt);
        Assert.True(result.CreatedAt <= DateTime.UtcNow);

        //Booking Not Found
        await Assert.ThrowsAsync<NotFoundException>(
            () => _bookingService.CancelBookingByIdAsync(Guid.NewGuid(), testUser.Id));

        //User Not Found
        await Assert.ThrowsAsync<NotFoundException>(
            () => _bookingService.CancelBookingByIdAsync(result.Id, Guid.NewGuid()));

        var booking = await _bookingRepository.GetBookingByIdAsync(result.Id);
        booking?.EventId = Guid.NewGuid();
        await _bookingRepository.SaveChangesAsync();

        //Event Not Found
        await Assert.ThrowsAsync<NotFoundException>(
            () => _bookingService.CancelBookingByIdAsync(result.Id, testUser.Id));
    }

    [Fact]
    public async Task CancelBookingByIdAsync_NotAdminOrNotCorrectUser_ThrowsLackOfRightsException()
    {
        var eventId = await CreateTestEventAsync();
        var userAdmin = await CreateTestUserAsync(role: UserRoles.Admin);
        var user1 = await CreateTestUserAsync(login: "Login 1");
        var user2 = await CreateTestUserAsync(login: "Login 2");
        var bookingByAdmin = await _bookingService.CreateBookingAsync(eventId, userAdmin.Id);
        var bookingByUser1 = await _bookingService.CreateBookingAsync(eventId, user1.Id);
        var bookingByUser2 = await _bookingService.CreateBookingAsync(eventId, user2.Id);

        // No one Cancelled
        Assert.Equal(BookingStatus.Pending, bookingByAdmin.Status);
        Assert.Equal(BookingStatus.Pending, bookingByUser1.Status);
        Assert.Equal(BookingStatus.Pending, bookingByUser2.Status);

        // Admin can Cancel his own Booking
        Assert.True(await _bookingService.CancelBookingByIdAsync(bookingByAdmin.Id, userAdmin.Id));
        bookingByAdmin = await _bookingService.GetBookingByIdAsync(bookingByAdmin.Id);
        Assert.Equal(BookingStatus.Cancelled, bookingByAdmin.Status);

        // Admin can Cancel foreign Booking
        Assert.True(await _bookingService.CancelBookingByIdAsync(bookingByUser1.Id, userAdmin.Id));
        bookingByUser1 = await _bookingService.GetBookingByIdAsync(bookingByUser1.Id);
        Assert.Equal(BookingStatus.Cancelled, bookingByUser1.Status);

        // User1 can NOT Cancel foreign Booking
        await Assert.ThrowsAsync<LackOfRightsException>(
            () => _bookingService.CancelBookingByIdAsync(bookingByUser2.Id, user1.Id));
        bookingByUser2 = await _bookingService.GetBookingByIdAsync(bookingByUser2.Id);
        Assert.NotEqual(BookingStatus.Cancelled, bookingByUser2.Status);
    }

    [Fact]
    public async Task CancelBookingByIdAsync_CorrectUserRights_SuccessfullyCancelAndReleaseSeats()
    {
        var eventId = await CreateTestEventAsync();
        var user = await CreateTestUserAsync();
        var @event = await _eventService.GetEventByIdAsync(eventId);

        // Seats count Not changed
        Assert.Equal(10, @event.AvailableSeats);

        // One seat booked
        var booking = await _bookingService.CreateBookingAsync(eventId, user.Id);
        @event = await _eventService.GetEventByIdAsync(eventId);
        Assert.Equal(9, @event.AvailableSeats);

        // Successfull Cancellation of Booking
        Assert.True(await _bookingService.CancelBookingByIdAsync(booking.Id, user.Id));
        booking = await _bookingService.GetBookingByIdAsync(booking.Id);
        Assert.Equal(BookingStatus.Cancelled, booking.Status);

        // Seat is released
        @event = await _eventService.GetEventByIdAsync(eventId);
        Assert.Equal(10, @event.AvailableSeats);
    }

    #endregion

    #region Concurrency Tests

    [Fact]
    public async Task CreateBookingAsync_ConcurrentRequests_DoesNotOverbookEvent()
    {
        const int totalSeats = 5;
        const int concurrentRequests = 20;
        var eventId = await CreateTestEventAsync(totalSeats: totalSeats);
        var testUser = await CreateTestUserAsync();

        var tasks = Enumerable.Range(0, concurrentRequests)
            .Select(_ => Task.Run(async () =>
            {
                using var scope = _serviceProvider.CreateScope();
                var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                try
                {
                    await bookingService.CreateBookingAsync(eventId, testUser.Id);
                    return true;
                }
                catch (NoAvailableSeatsException)
                {
                    return false;
                }
            }));

        var results = await Task.WhenAll(tasks);

        var successCount = results.Count(r => r);
        Assert.Equal(totalSeats, successCount);
    }

    [Fact]
    public async Task CreateBookingAsync_ConcurrentRequests_AllSuccessfulBookingsHaveUniqueIds()
    {
        const int totalSeats = 10;
        const int concurrentRequests = 10;
        var eventId = await CreateTestEventAsync(totalSeats: totalSeats);
        var testUser = await CreateTestUserAsync();
        var bookingIds = new System.Collections.Concurrent.ConcurrentBag<Guid>();

        var tasks = Enumerable.Range(0, concurrentRequests)
            .Select(_ => Task.Run(async () =>
            {
                using var scope = _serviceProvider.CreateScope();
                var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                var booking = await bookingService.CreateBookingAsync(eventId, testUser.Id);
                bookingIds.Add(booking.Id);
            }));

        await Task.WhenAll(tasks);

        Assert.Equal(totalSeats, bookingIds.Distinct().Count());
    }

    #endregion
}
