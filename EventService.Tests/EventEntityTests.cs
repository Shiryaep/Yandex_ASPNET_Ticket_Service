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
    }
}
