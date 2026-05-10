using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Yandex_ASPNET_Ticket_Service.Models;

namespace EventService_Tests
{
    // Need to test custom validator behaviour
    public class EventEntityTests
    {
        // Метод_Состояние_ОжидаемыйРезультат
        [Fact]
        public void Event_EndAtEarlierThanStartAt_ValidationFails()
        {
            // Arrange
            var startDate = DateTime.Now;
            var endDate = startDate.AddHours(-2);

            var @event = new Event
            {
                Id = Guid.NewGuid(),
                Title = "Yandex Asp.Net Study",
                Description = "With great power comes great responsibility.",
                StartAt = startDate,
                EndAt = endDate
            };

            // Act
            var validationContext = new ValidationContext(@event, null, null);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(@event, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, v =>
                v.ErrorMessage == "EndAt must be later than StartAt.");
        }

        [Fact]
        public void Event_EndAtLaterThanStartAt_ValidationSucceed()
        {
            // Arrange
            var startDate = DateTime.Now;
            var endDate = startDate.AddHours(2);

            var @event = new Event
            {
                Id = Guid.NewGuid(),
                Title = "Yandex Asp.Net Study",
                Description = "With great power comes great responsibility.",
                StartAt = startDate,
                EndAt = endDate
            };

            // Act
            var validationContext = new ValidationContext(@event, null, null);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(@event, validationContext, validationResults, true);

            // Assert
            Assert.True(isValid);
            Assert.DoesNotContain(validationResults, v =>
                v.ErrorMessage == "EndAt must be later than StartAt.");
        }

        [Fact]
        public void Event_EndAtEqualToStartAt_ValidationFails()
        {
            // Arrange
            var startDate = DateTime.Now;
            var endDate = startDate;

            var @event = new Event
            {
                Id = Guid.NewGuid(),
                Title = "Yandex Asp.Net Study",
                Description = "With great power comes great responsibility.",
                StartAt = startDate,
                EndAt = endDate
            };

            // Act
            var validationContext = new ValidationContext(@event, null, null);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(@event, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, v =>
                v.ErrorMessage == "EndAt must be later than StartAt.");
        }

        [Fact]
        public void Event_StartAtNull_ValidationFails()
        {
            // Arrange
            var @event = new Event
            {
                Id = Guid.NewGuid(),
                Title = "Yandex Asp.Net Study",
                Description = "With great power comes great responsibility.",
                StartAt = null,
                EndAt = DateTime.Now.AddHours(2)
            };

            // Act
            var validationContext = new ValidationContext(@event, null, null);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(@event, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, v =>
                v.ErrorMessage == "There is no Start At!");
        }

        [Fact]
        public void Event_EndAtNull_ValidationFails()
        {
            // Arrange
            var @event = new Event
            {
                Id = Guid.NewGuid(),
                Title = "Yandex Asp.Net Study",
                Description = "With great power comes great responsibility.",
                StartAt = DateTime.Now,
                EndAt = null
            };

            // Act
            var validationContext = new ValidationContext(@event, null, null);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(@event, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, v =>
                v.ErrorMessage == "There is no End At!");
        }

        [Fact]
        public void Event_TryReserveSeats_WhenAvailable_DecreasesAvailableSeats()
        {
            // Arrange
            var @event = new Event
            {
                Id = Guid.NewGuid(),
                Title = "Test",
                Description = "Test",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(2),
                TotalSeats = 10,
                AvailableSeats = 5
            };

            // Act
            var result = @event.TryReserveSeats(3);

            // Assert
            Assert.True(result);
            Assert.Equal(2, @event.AvailableSeats);
        }

        [Fact]
        public void Event_TryReserveSeats_WhenNotAvailable_ReturnsFalse()
        {
            // Arrange
            var @event = new Event
            {
                Id = Guid.NewGuid(),
                Title = "Test",
                Description = "Test",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(2),
                TotalSeats = 10,
                AvailableSeats = 2
            };

            // Act
            var result = @event.TryReserveSeats(3);

            // Assert
            Assert.False(result);
            Assert.Equal(2, @event.AvailableSeats); // unchanged
        }

        [Fact]
        public void Event_ReleaseSeats_IncreasesAvailableSeats()
        {
            // Arrange
            var @event = new Event
            {
                Id = Guid.NewGuid(),
                Title = "Test",
                Description = "Test",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(2),
                TotalSeats = 10,
                AvailableSeats = 5
            };

            // Act
            @event.ReleaseSeats(2);

            // Assert
            Assert.Equal(7, @event.AvailableSeats);
        }

        [Fact]
        public void Event_ReleaseSeats_DoesNotExceedTotalSeats()
        {
            // Arrange
            var @event = new Event
            {
                Id = Guid.NewGuid(),
                Title = "Test",
                Description = "Test",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(2),
                TotalSeats = 10,
                AvailableSeats = 9
            };

            // Act
            @event.ReleaseSeats(5);

            // Assert
            // ReleaseSeats does not increase AvailableSeats beyond TotalSeats
            // Since AvailableSeats + count > TotalSeats, the condition fails and AvailableSeats remains unchanged
            Assert.Equal(9, @event.AvailableSeats);
            Assert.True(@event.AvailableSeats <= @event.TotalSeats);
        }
    }
}