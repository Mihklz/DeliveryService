using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DeliveryService.Tests
{
    public class OrderFilterTests
    {
        [Fact]
        public void FilterOrders_ShouldReturnCorrectOrders()
        {
            // Arrange
            var orders = new List<DeliveryOrder>
    {
        new DeliveryOrder { OrderId = "1", District = "Downtown", DeliveryTime = new DateTime(2024, 10, 25, 14, 30, 0) },
        new DeliveryOrder { OrderId = "2", District = "Downtown", DeliveryTime = new DateTime(2024, 10, 25, 14, 50, 0) },
        new DeliveryOrder { OrderId = "3", District = "Uptown", DeliveryTime = new DateTime(2024, 10, 25, 16, 0, 0) },
        new DeliveryOrder { OrderId = "4", District = "Downtown", DeliveryTime = new DateTime(2024, 10, 25, 14, 40, 0) }
    };

            string cityDistrict = "Downtown";
            DateTime firstDeliveryTime = new DateTime(2024, 10, 25, 14, 30, 0);

            // Act
            var filteredOrders = orders
                .Where(o => o.District == cityDistrict && o.DeliveryTime >= firstDeliveryTime && o.DeliveryTime <= firstDeliveryTime.AddMinutes(10))
                .ToList();

            // Добавляем отладочный вывод для проверки, какие заказы были отфильтрованы
            Console.WriteLine($"Фактически отфильтровано заказов: {filteredOrders.Count}");
            foreach (var order in filteredOrders)
            {
                Console.WriteLine($"OrderId: {order.OrderId}, DeliveryTime: {order.DeliveryTime}");
            }

            // Assert
            Assert.Equal(2, filteredOrders.Count);  // Ожидаем 2 заказа
            Assert.Contains(filteredOrders, o => o.OrderId == "1");
            Assert.Contains(filteredOrders, o => o.OrderId == "4");
        }

    }
}
