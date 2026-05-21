//using System.Collections.Concurrent;
//using Moq;
//using Yandex_ASPNET_Ticket_Service.Models;
//using Yandex_ASPNET_Ticket_Service.Services.BookingServices;
//using Yandex_ASPNET_Ticket_Service.Services.EventServices;
//using Yandex_ASPNET_Ticket_Service.Storage;

//namespace EventService_Tests;

//public class BookingServiceTests
//{
//    private readonly BookingService _bookingService;
//    private readonly InMemoryBookingStorage _bookingStorage;
//    private readonly Mock<IEventService> _eventServiceMock;

//    public BookingServiceTests()
//    {
//        _bookingStorage = new InMemoryBookingStorage();
//        _eventServiceMock = new Mock<IEventService>();
//        _bookingService = new BookingService(_bookingStorage, _eventServiceMock.Object);
//    }

//    private static Event CreateTestEvent(Guid eventId, int totalSeats = 10, int? availableSeats = null)
//    {
//        return new Event
//        {
//            Id = eventId,
//            Title = "Test Event",
//            Description = "Test Description",
//            StartAt = DateTime.UtcNow.AddDays(1),
//            EndAt = DateTime.UtcNow.AddDays(2),
//            TotalSeats = totalSeats,
//            AvailableSeats = availableSeats ?? totalSeats
//        };
//    }

//    [Fact]
//    public async Task CreateBooking_ForExistingEvent_ReturnsBookingWithPendingStatus()
//    {
//        // Arrange
//        var eventId = Guid.NewGuid();
//        var testEvent = CreateTestEvent(eventId);
//        _eventServiceMock.Setup(es => es.GetEvent(eventId)).Returns(testEvent);
//        _eventServiceMock.Setup(es => es.UpdateEvent(eventId, It.IsAny<Event>())).Verifiable();

//        // Act
//        var booking = await _bookingService.CreateBookingAsync(eventId);

//        // Assert
//        Assert.NotNull(booking);
//        Assert.Equal(eventId, booking.EventId);
//        Assert.Equal(BookingStatus.Pending, booking.Status);
//        Assert.NotEqual(Guid.Empty, booking.Id);
//        Assert.True(booking.CreatedAt <= DateTime.UtcNow);
//        _eventServiceMock.Verify(es => es.UpdateEvent(eventId, It.IsAny<Event>()), Times.Once);
//    }

//    [Fact]
//    public async Task CreateMultipleBookings_ForSameEvent_AllHaveUniqueIds()
//    {
//        // Arrange
//        var eventId = Guid.NewGuid();
//        var testEvent = CreateTestEvent(eventId);
//        _eventServiceMock.Setup(es => es.GetEvent(eventId)).Returns(testEvent);
//        _eventServiceMock.Setup(es => es.UpdateEvent(eventId, It.IsAny<Event>())).Verifiable();

//        var bookingIds = new HashSet<Guid>();
//        var eventsIds = new HashSet<Guid>();

//        // Act
//        for (int i = 0; i < 5; i++)
//        {
//            var booking = await _bookingService.CreateBookingAsync(eventId);
//            bookingIds.Add(booking.Id);
//            eventsIds.Add(booking.EventId);
//        }

//        // Assert
//        Assert.Equal(5, bookingIds.Count);
//        Assert.Single(eventsIds);
//        Assert.Equal(eventsIds.FirstOrDefault(), eventId);
//        _eventServiceMock.Verify(es => es.UpdateEvent(eventId, It.IsAny<Event>()), Times.Exactly(5));
//    }

//    [Fact]
//    public async Task GetBookingById_ExistingBooking_ReturnsCorrectInfo()
//    {
//        // Arrange
//        var eventId = Guid.NewGuid();
//        var testEvent = CreateTestEvent(eventId);
//        _eventServiceMock.Setup(es => es.GetEvent(eventId)).Returns(testEvent);
//        _eventServiceMock.Setup(es => es.UpdateEvent(eventId, It.IsAny<Event>())).Verifiable();

//        var createdBooking = await _bookingService.CreateBookingAsync(eventId);

//        // Act
//        var retrievedBooking = await _bookingService.GetBookingByIdAsync(createdBooking.Id);

//        // Assert
//        Assert.NotNull(retrievedBooking);
//        Assert.Equal(createdBooking.Id, retrievedBooking.Id);
//        Assert.Equal(createdBooking.EventId, retrievedBooking.EventId);
//        Assert.Equal(createdBooking.Status, retrievedBooking.Status);
//        Assert.Equal(createdBooking.CreatedAt, retrievedBooking.CreatedAt);
//    }

//    [Fact]
//    public async Task GetBookingById_AfterStatusChange_ReflectsUpdatedStatus()
//    {
//        // Arrange
//        var eventId = Guid.NewGuid();
//        var testEvent = CreateTestEvent(eventId);
//        _eventServiceMock.Setup(es => es.GetEvent(eventId)).Returns(testEvent);
//        _eventServiceMock.Setup(es => es.UpdateEvent(eventId, It.IsAny<Event>())).Verifiable();

//        var booking = await _bookingService.CreateBookingAsync(eventId);

//        booking.Status = BookingStatus.Confirmed;
//        booking.ProcessedAt = DateTime.UtcNow;
//        await _bookingStorage.UpdateAsync(booking);

//        // Act
//        var retrievedBooking = await _bookingService.GetBookingByIdAsync(booking.Id);

//        // Assert
//        Assert.NotNull(retrievedBooking);
//        Assert.Equal(BookingStatus.Confirmed, retrievedBooking.Status);
//        Assert.NotNull(retrievedBooking.ProcessedAt);
//    }

//    [Fact]
//    public async Task GetBookingById_NonExistentId_ReturnsNull()
//    {
//        // Arrange
//        var nonExistentId = Guid.NewGuid();

//        // Act
//        var booking = await _bookingService.GetBookingByIdAsync(nonExistentId);

//        // Assert
//        Assert.Null(booking);
//    }

//    [Fact]
//    public async Task CreateBooking_DecreasesAvailableSeatsByOne()
//    {
//        // Arrange
//        var eventId = Guid.NewGuid();
//        var testEvent = CreateTestEvent(eventId, totalSeats: 10, availableSeats: 10);
//        _eventServiceMock.Setup(es => es.GetEvent(eventId)).Returns(testEvent);
//        Event? capturedEvent = null;
//        _eventServiceMock.Setup(es => es.UpdateEvent(eventId, It.IsAny<Event>()))
//            .Callback<Guid, Event>((id, ev) => capturedEvent = ev)
//            .Verifiable();

//        // Act
//        var booking = await _bookingService.CreateBookingAsync(eventId);

//        // Assert
//        Assert.NotNull(booking);
//        _eventServiceMock.Verify(es => es.UpdateEvent(eventId, It.IsAny<Event>()), Times.Once);
//        Assert.NotNull(capturedEvent);
//        Assert.Equal(9, capturedEvent.AvailableSeats); // decreased by 1
//        Assert.Equal(10, capturedEvent.TotalSeats); // unchanged
//    }

//    [Fact]
//    public async Task CreateBooking_ForNonExistingEvent_ThrowsArgumentException()
//    {
//        // Arrange
//        var eventId = Guid.NewGuid();
//        _eventServiceMock.Setup(es => es.GetEvent(eventId)).Returns((Event?)null);

//        // Act & Assert
//        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _bookingService.CreateBookingAsync(eventId));
//        Assert.Contains(eventId.ToString(), exception.Message);
//        _eventServiceMock.Verify(es => es.UpdateEvent(It.IsAny<Guid>(), It.IsAny<Event>()), Times.Never);
//    }

//    [Fact]
//    public async Task CreateBooking_NoAvailableSeats_ThrowsNoAvailableSeatsException()
//    {
//        // Arrange
//        var eventId = Guid.NewGuid();
//        var testEvent = CreateTestEvent(eventId, totalSeats: 10, availableSeats: 0);
//        _eventServiceMock.Setup(es => es.GetEvent(eventId)).Returns(testEvent);

//        // Act & Assert
//        await Assert.ThrowsAsync<NoAvailableSeatsException>(() => _bookingService.CreateBookingAsync(eventId));
//        _eventServiceMock.Verify(es => es.UpdateEvent(eventId, It.IsAny<Event>()), Times.Never);
//    }

//    [Fact]
//    public void Booking_Confirm_SetsStatusConfirmedAndProcessedAt()
//    {
//        // Arrange
//        var booking = new Booking(Guid.NewGuid());
//        var before = DateTime.UtcNow;

//        // Act
//        booking.Confirm();

//        // Assert
//        Assert.Equal(BookingStatus.Confirmed, booking.Status);
//        Assert.NotNull(booking.ProcessedAt);
//        Assert.True(booking.ProcessedAt >= before && booking.ProcessedAt <= DateTime.UtcNow);
//    }

//    [Fact]
//    public void Booking_Reject_SetsStatusRejectedAndProcessedAt()
//    {
//        // Arrange
//        var booking = new Booking(Guid.NewGuid());
//        var before = DateTime.UtcNow;

//        // Act
//        booking.Reject();

//        // Assert
//        Assert.Equal(BookingStatus.Rejected, booking.Status);
//        Assert.NotNull(booking.ProcessedAt);
//        Assert.True(booking.ProcessedAt >= before && booking.ProcessedAt <= DateTime.UtcNow);
//    }

//    [Fact]
//    public async Task CreateBooking_ConcurrentOverbooking_OnlyFiveSuccesses()
//    {
//        // Arrange
//        var eventId = Guid.NewGuid();
//        var testEvent = CreateTestEvent(eventId, totalSeats: 5, availableSeats: 5);
//        _eventServiceMock.Setup(es => es.GetEvent(eventId)).Returns(testEvent);
//        _eventServiceMock.Setup(es => es.UpdateEvent(eventId, It.IsAny<Event>()))
//            .Callback<Guid, Event>((id, ev) => testEvent = ev);

//        var tasks = new List<Task<Booking?>>();
//        // Concurrent because of Tasks
//        // Standard collections will get less than 15 exceptions
//        var exceptions = new ConcurrentBag<NoAvailableSeatsException>();
//        var successfulBookings = new ConcurrentBag<Booking>();

//        // Act
//        // Prepare 20 parallel requests
//        for (int i = 0; i < 20; i++)
//        {
//            tasks.Add(Task.Run(async () =>
//            {
//                try
//                {
//                    var booking = await _bookingService.CreateBookingAsync(eventId);
//                    successfulBookings.Add(booking);
//                    return booking;
//                }
//                catch (NoAvailableSeatsException ex)
//                {
//                    exceptions.Add(ex);
//                    return null;
//                }
//            }));
//        }

//        var results = await Task.WhenAll(tasks);

//        // Assert
//        Assert.Equal(5, successfulBookings.Count); // There must be exactly 5 successful bookings
//        _eventServiceMock.Verify(es => es.UpdateEvent(eventId, It.IsAny<Event>()), Times.Exactly(5));

//        Assert.Equal(15, exceptions.Count); // The other 15 should get an exception
//        Assert.Equal(0, testEvent.AvailableSeats); // All seats must be reserved
//        Assert.Equal(20, successfulBookings.Count + exceptions.Count); // Total number of requests = 20
//    }

//    [Fact]
//    public async Task CreateBooking_ConcurrentRequests_AllIdsAreUnique()
//    {
//        // Arrange
//        var eventId = Guid.NewGuid();
//        var testEvent = CreateTestEvent(eventId, totalSeats: 100, availableSeats: 100);
//        _eventServiceMock.Setup(es => es.GetEvent(eventId)).Returns(testEvent);
//        _eventServiceMock.Setup(es => es.UpdateEvent(eventId, It.IsAny<Event>()))
//            .Callback<Guid, Event>((id, ev) => testEvent = ev);

//        const int concurrentRequests = 50;
//        var tasks = new List<Task<Booking>>();
//        var idSet = new HashSet<Guid>();

//        // Act
//        // Prepare 50 parallel requests
//        for (int i = 0; i < concurrentRequests; i++)
//        {
//            tasks.Add(Task.Run(() => _bookingService.CreateBookingAsync(eventId)));
//        }

//        var bookings = await Task.WhenAll(tasks);

//        // Prepare List of all Ids
//        foreach (var booking in bookings)
//        {
//            idSet.Add(booking.Id);
//        }

//        // Assert
//        Assert.Equal(concurrentRequests, idSet.Count); // All Ids should be unique
//        Assert.Equal(concurrentRequests, bookings.Length); // No exceptiosn
//        Assert.Equal(100 - concurrentRequests, testEvent.AvailableSeats); // Reserved correct amount of seats
//        _eventServiceMock.Verify(es => es.UpdateEvent(eventId, It.IsAny<Event>()), Times.Exactly(concurrentRequests));
//    }
//}