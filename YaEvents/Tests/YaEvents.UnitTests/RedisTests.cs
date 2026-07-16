using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using YaEvents.Application.DTO;
using YaEvents.Application.Services;
using YaEvents.Application.Services.EventServices;
using YaEvents.Domain;
using YaEvents.Domain.Exceptions;
using YaEvents.Infrastructure.DataAccess;
using YaEvents.Infrastructure.Repositories;
using YaEvents.Infrastructure.Services;
using YaEvents.Infrastructure.Settings;

namespace YaEvents.UnitTests
{
    public class RedisTests
    {
        private AppDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private IOptions<RedisCacheSettings> CreateSettings()
        {
            return Options.Create(new RedisCacheSettings
            {
                EventsGetEventByIdTTLSeconds = 60,
                EventsGetTopEventsTTLMinutes = 10
            });
        }

        [Fact]
        public async Task GetEventByIdAsync_CacheHit_ReturnsFromCache_DbNotCalled()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var eventId = Guid.NewGuid();
            var cachedEvent = new EventInfoDto()
            {
                Id = eventId,
                Title = "Title",
                Description = "Test Event Description",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(2),
                AvailableSeats = 10,
                TotalSeats = 10
            };

            var cacheKey = $"event:{eventId}";

            var mockCache = new Mock<ICacheService>();
            mockCache.Setup(c => c.GetAsync<EventInfoDto>(cacheKey)).ReturnsAsync(cachedEvent);
            var cacheHelper = new RedisCacheHelper(mockCache.Object, CreateSettings());

            var repo = new EventRepository(context);
            var service = new EventService(repo, mockCache.Object, cacheHelper);

            // Act
            var result = await service.GetEventByIdAsync(eventId, CancellationToken.None);

            // Assert
            Assert.Same(cachedEvent, result);

            // Проверяем, что кеш был запрошен ровно один раз
            mockCache.Verify(c => c.GetAsync<EventInfoDto>(cacheKey), Times.Once);

            // Проверяем, что запись в кеш не производилась (так как был hit)
            mockCache.Verify(c => c.SetAsync<EventInfoDto>(It.IsAny<string>(), It.IsAny<EventInfoDto>(), It.IsAny<TimeSpan>()), Times.Never);
        }

        [Fact]
        public async Task GetEventByIdAsync_CacheMiss_FetchesFromDb_StoresInCache()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var dbEvent = Event.Create("Title", "Test Event Description", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 10);
            context.Events.Add(dbEvent);
            await context.SaveChangesAsync();

            var eventId = dbEvent.Id;
            var cacheKey = $"event:{eventId}";
            var ttl = TimeSpan.FromSeconds(60);

            var mockCache = new Mock<ICacheService>();
            mockCache.Setup(c => c.GetAsync<EventInfoDto>(cacheKey)).ReturnsAsync((EventInfoDto?)null);
            var cacheHelper = new RedisCacheHelper(mockCache.Object, CreateSettings());

            var repo = new EventRepository(context);
            var service = new EventService(repo, mockCache.Object, cacheHelper);

            // Act
            var result = await service.GetEventByIdAsync(eventId, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dbEvent.Id, result.Id);

            mockCache.Verify(c => c.GetAsync<EventInfoDto>(cacheKey), Times.Once);

            mockCache.Verify(c => c.SetAsync<EventInfoDto>(
                cacheKey,
                It.Is<EventInfoDto>(e => e.Id == dbEvent.Id),
                ttl), Times.Once);
        }

        [Fact]
        public async Task UpdateEventAsync_ValidEventUpdatesEvent_UpdatesCacheTwiceOnGetAndOnUpdate()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var originalEvent = Event.Create("Original Title", "Test Event Description", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 10);
            context.Events.Add(originalEvent);
            await context.SaveChangesAsync();

            var eventId = originalEvent.Id;
            var updatedTitle = "Updated Title";
            var updateDto = new UpdateEventDto
            {
                Title = updatedTitle,
                Description = "New description",
                StartAt = DateTime.UtcNow.AddHours(2),
                EndAt = DateTime.UtcNow.AddHours(4)
            };

            var mockCache = new Mock<ICacheService>();
            var cacheHelper = new RedisCacheHelper(mockCache.Object, CreateSettings());

            var repo = new EventRepository(context);
            var service = new EventService(repo, mockCache.Object, cacheHelper);

            // Act
            var result = await service.UpdateEventAsync(eventId, updateDto, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(eventId, result.Id);
            Assert.Equal(updatedTitle, result.Title);

            var dbUpdatedEvent = await context.Events.FindAsync(eventId);
            Assert.NotNull(dbUpdatedEvent);
            Assert.Equal(updatedTitle, dbUpdatedEvent.Title);

            mockCache.Verify(c => c.SetAsync<EventInfoDto>(
                $"event:{eventId}",
                It.Is<EventInfoDto>(e => e.Title == updatedTitle),
                It.IsAny<TimeSpan>()), Times.Once);
        }

        [Fact]
        public async Task UpdateEventAsync_EventNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var eventId = Guid.NewGuid();
            var updateDto = new UpdateEventDto
            {
                Title = "Title",
                Description = "Desc",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(2)
            };

            var mockCache = new Mock<ICacheService>();
            var cacheHelper = new RedisCacheHelper(mockCache.Object, CreateSettings());
            var repo = new EventRepository(context);
            var service = new EventService(repo, mockCache.Object, cacheHelper);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.UpdateEventAsync(eventId, updateDto, CancellationToken.None));

            mockCache.Verify(c => c.SetAsync<EventInfoDto>(It.IsAny<string>(), It.IsAny<EventInfoDto>(), It.IsAny<TimeSpan>()), Times.Never);
        }

        [Fact]
        public async Task DeleteEventAsync_ValidEvent_DeletesEvent_SavesChanges()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var existingEvent = Event.Create("Title", "Description", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 10);
            var eventId = existingEvent.Id;
            var cacheKey = $"event:{eventId}";

            context.Events.Add(existingEvent);
            await context.SaveChangesAsync();

            var mockCache = new Mock<ICacheService>();
            var cacheHelper = new RedisCacheHelper(mockCache.Object, CreateSettings());

            var repo = new EventRepository(context);
            var service = new EventService(repo, mockCache.Object, cacheHelper);

            // Act
            var result = await service.DeleteEventAsync(eventId, CancellationToken.None);

            // Assert
            Assert.True(result);
            mockCache.Verify(i => i.RemoveAsync(cacheKey), Times.Once);
            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.GetEventByIdAsync(eventId, CancellationToken.None));
        }
    }
}