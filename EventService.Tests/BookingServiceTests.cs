using Yandex_ASPNET_Ticket_Service.Models;
using Yandex_ASPNET_Ticket_Service.Services.BookingServices;
using Yandex_ASPNET_Ticket_Service.Storage;

namespace EventService_Tests;

public class BookingServiceTests
{
    private readonly BookingService _bookingService;
    private readonly InMemoryBookingStorage _bookingStorage;

    public BookingServiceTests()
    {
        _bookingStorage = new InMemoryBookingStorage();
        _bookingService = new BookingService(_bookingStorage);
    }

    [Fact]
    public async Task CreateBooking_ForExistingEvent_ReturnsBookingWithPendingStatus()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        // Act
        var booking = await _bookingService.CreateBookingAsync(eventId);

        // Assert
        Assert.NotNull(booking);
        Assert.Equal(eventId, booking.EventId);
        Assert.Equal(BookingStatus.Pending, booking.Status);
        Assert.NotEqual(Guid.Empty, booking.Id);
        Assert.True(booking.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public async Task CreateMultipleBookings_ForSameEvent_AllHaveUniqueIds()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var bookingIds = new HashSet<Guid>();
        var eventsIds = new HashSet<Guid>();

        // Act
        for (int i = 0; i < 5; i++)
        {
            var booking = await _bookingService.CreateBookingAsync(eventId);
            bookingIds.Add(booking.Id);
            eventsIds.Add(booking.EventId);
        }

        // Assert
        Assert.Equal(5, bookingIds.Count);
        Assert.Single(eventsIds);
        Assert.Equal(eventsIds.FirstOrDefault(), eventId);
    }

    [Fact]
    public async Task GetBookingById_ExistingBooking_ReturnsCorrectInfo()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var createdBooking = await _bookingService.CreateBookingAsync(eventId);

        // Act
        var retrievedBooking = await _bookingService.GetBookingByIdAsync(createdBooking.Id);

        // Assert
        Assert.NotNull(retrievedBooking);
        Assert.Equal(createdBooking.Id, retrievedBooking.Id);
        Assert.Equal(createdBooking.EventId, retrievedBooking.EventId);
        Assert.Equal(createdBooking.Status, retrievedBooking.Status);
        Assert.Equal(createdBooking.CreatedAt, retrievedBooking.CreatedAt);
    }

    [Fact]
    public async Task GetBookingById_AfterStatusChange_ReflectsUpdatedStatus()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var booking = await _bookingService.CreateBookingAsync(eventId);

        booking.Status = BookingStatus.Confirmed;
        booking.ProcessedAt = DateTime.UtcNow;
        await _bookingStorage.UpdateAsync(booking);

        // Act
        var retrievedBooking = await _bookingService.GetBookingByIdAsync(booking.Id);

        // Assert
        Assert.NotNull(retrievedBooking);
        Assert.Equal(BookingStatus.Confirmed, retrievedBooking.Status);
        Assert.NotNull(retrievedBooking.ProcessedAt);
    }

    [Fact]
    public async Task GetBookingById_NonExistentId_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var booking = await _bookingService.GetBookingByIdAsync(nonExistentId);

        // Assert
        Assert.Null(booking);
    }
}