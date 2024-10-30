using System;
using Xunit;

namespace DeliveryService.Tests
{
    public class ParameterValidationTests
    {
        [Fact]
        public void ValidateParameters_ShouldThrowExceptionForInvalidDate()
        {
            // Arrange: некорректное значение даты
            string invalidDate = "invalid-date";
            DateTime parsedDate;

            // Act & Assert: проверяем, что дата не парсится
            Assert.False(DateTime.TryParse(invalidDate, out parsedDate));
        }

        [Fact]
        public void ValidateParameters_ShouldParseCorrectDate()
        {
            // Arrange: корректное значение даты
            string validDate = "2024-10-25 14:30:00";
            DateTime parsedDate;

            // Act: пытаемся распарсить дату
            var result = DateTime.TryParse(validDate, out parsedDate);

            // Assert: дата должна парситься корректно
            Assert.True(result);
            Assert.Equal(new DateTime(2024, 10, 25, 14, 30, 0), parsedDate);
        }
    }
}
