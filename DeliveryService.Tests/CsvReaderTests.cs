using System.Collections.Generic;
using System.IO;
using Xunit;
using CsvHelper;
using System.Globalization;

namespace DeliveryService.Tests
{
    public class CsvReaderTests
    {
        [Fact]
        public void ReadOrders_ShouldReturnCorrectOrderList()
        {
            // Arrange: создаем строку с данными в формате CSV
            var csvData = "OrderId,Weight,District,DeliveryTime\n" +
                          "1,10.5,Downtown,2024-10-25 14:30:00\n" +
                          "2,12.0,Downtown,2024-10-25 14:50:00";

            using (var reader = new StringReader(csvData))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // Act: читаем данные из CSV
                var orders = csv.GetRecords<DeliveryOrder>().ToList();

                // Добавляем вывод данных для отладки
                Console.WriteLine($"Количество заказов: {orders.Count}");
                foreach (var order in orders)
                {
                    Console.WriteLine($"OrderId: {order.OrderId}, Weight: {order.Weight}, District: {order.District}, DeliveryTime: {order.DeliveryTime}");
                }

                // Assert: проверяем количество заказов
                Assert.NotNull(orders);
                Assert.Equal(2, orders.Count());  // Ожидаем 2 заказа
                Assert.Contains(orders, o => o.OrderId == "1");
                Assert.Contains(orders, o => o.OrderId == "2");
            }
        }

    }
}

