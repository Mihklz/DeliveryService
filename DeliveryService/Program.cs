using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using Microsoft.Extensions.Logging;
using Serilog;

namespace DeliveryService
{
    public class DeliveryOrder
    {
        public string OrderId { get; set; }
        public double Weight { get; set; }
        public string District { get; set; }
        public DateTime DeliveryTime { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Получаем параметры из командной строки
            string cityDistrict = GetCommandLineArgument(args, "_cityDistrict");
            string firstDeliveryDateTimeStr = GetCommandLineArgument(args, "_firstDeliveryDateTime");
            string logFilePath = GetCommandLineArgument(args, "_deliveryLog");
            string resultFilePath = GetCommandLineArgument(args, "_deliveryOrder");

            // Валидация входных параметров
            if (string.IsNullOrEmpty(cityDistrict))
            {
                Console.WriteLine("Ошибка: Не указан район доставки (_cityDistrict)");
                return;
            }

            if (!DateTime.TryParseExact(firstDeliveryDateTimeStr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime firstDeliveryTime))
            {
                Console.WriteLine("Ошибка: Некорректное время первой доставки (_firstDeliveryDateTime)");
                return;
            }

            if (string.IsNullOrEmpty(logFilePath))
            {
                Console.WriteLine("Ошибка: Не указан путь к файлу логов (_deliveryLog)");
                return;
            }

            if (string.IsNullOrEmpty(resultFilePath))
            {
                Console.WriteLine("Ошибка: Не указан путь к файлу с результатами (_deliveryOrder)");
                return;
            }

            // Конфигурация Serilog для логирования в файл
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(logFilePath)
                .CreateLogger();

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddSerilog();
            });
            var logger = loggerFactory.CreateLogger<Program>();

            logger.LogInformation("Начало работы приложения Delivery Service");

            // Чтение данных из CSV файла
            var filePath = @"\\Mac\Home\Desktop\DTZ\DeliveryService\DeliveryService\orders.csv"; // Укажи здесь правильный путь к orders.csv
            var orders = ReadOrders(filePath);

            // Фильтрация заказов
            var filteredOrders = orders
                .Where(o => o.District == cityDistrict && o.DeliveryTime >= firstDeliveryTime && o.DeliveryTime < firstDeliveryTime.AddMinutes(30))
                .ToList();

            if (orders == null || !orders.Any())
            {
                logger.LogWarning("Заказы не были найдены.");
                return;
            }

            // Вывод всех загруженных заказов на консоль для проверки
            Console.WriteLine("Загруженные заказы:");
            foreach (var order in orders)
            {
                Console.WriteLine($"OrderId: {order.OrderId}, Weight: {order.Weight}, District: {order.District}, DeliveryTime: {order.DeliveryTime}");
            }

            logger.LogInformation("Данные заказов успешно загружены.");

            // Подсчет количества заказов в каждом районе
            var districtOrderCounts = orders
                .GroupBy(o => o.District)
                .Select(g => new { District = g.Key, OrderCount = g.Count() })
                .ToDictionary(g => g.District, g => g.OrderCount);

            // Логируем количество заказов в каждом районе
            foreach (var district in districtOrderCounts)
            {
                Console.WriteLine($"Район {district.Key} имеет {district.Value} заказов.");
            }

            Console.WriteLine("Заказы перед фильтрацией:");
            foreach (var order in orders)
            {
                Console.WriteLine($"OrderId: {order.OrderId}, District: {order.District}, DeliveryTime: {order.DeliveryTime}");
            }

            // Отладочная информация для фильтрации
            Console.WriteLine($"Ищем заказы для района {cityDistrict} с доставкой от {firstDeliveryTime} до {firstDeliveryTime.AddMinutes(30)}");

            foreach (var order in orders)
            {
                Console.WriteLine($"Проверяем заказ {order.OrderId}: район {order.District}, время доставки {order.DeliveryTime}");
                Console.WriteLine($"Сравниваем район заказа {order.District} с районом фильтрации {cityDistrict}");
                Console.WriteLine($"Сравниваем время доставки заказа {order.DeliveryTime} с интервалом {firstDeliveryTime} - {firstDeliveryTime.AddMinutes(30)}");

                if (order.District == cityDistrict
                    && order.DeliveryTime >= firstDeliveryTime
                    && order.DeliveryTime < firstDeliveryTime.AddMinutes(30))
                {
                    Console.WriteLine($"Заказ {order.OrderId} соответствует условиям.");
                }
                else
                {
                    Console.WriteLine($"Заказ {order.OrderId} не соответствует условиям.");
                }
            }

            // Фильтрация заказов по району и времени (без порога по количеству обращений)
            var filteredOrdersResult = orders
                .Where(o => o.District == cityDistrict
                    && o.DeliveryTime >= firstDeliveryTime
                    && o.DeliveryTime < firstDeliveryTime.AddMinutes(30))
                .ToList();

            if (filteredOrdersResult.Any())
            {
                Console.WriteLine("Отфильтрованные заказы:");
                foreach (var order in filteredOrdersResult)
                {
                    Console.WriteLine($"OrderId: {order.OrderId}, District: {order.District}, DeliveryTime: {order.DeliveryTime}");
                }

                WriteOrdersToFile(filteredOrdersResult, resultFilePath);
                logger.LogInformation($"Найдено {filteredOrdersResult.Count} заказов для района {cityDistrict} в ближайшие 30 минут.");
            }
            else
            {
                Console.WriteLine("Нет заказов, соответствующих условиям фильтрации.");
                logger.LogWarning($"Не найдено заказов для района {cityDistrict}.");
            }

            logger.LogInformation("Приложение завершило работу.");

            Console.WriteLine("Нажмите любую клавишу для завершения...");
            Console.ReadKey();


        }

        public static List<DeliveryOrder> ReadOrders(string filePath)
        {
            try
            {
                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                return csv.GetRecords<DeliveryOrder>().ToList();
            }
            catch (Exception ex)
            {
                Log.Error($"Ошибка при чтении файла: {ex.Message}");
                return null;
            }
        }

        public static List<DeliveryOrder> FilterOrders(List<DeliveryOrder> orders, string district, DateTime firstDeliveryTime)
        {
            return orders
                .Where(o => o.District == district && o.DeliveryTime >= firstDeliveryTime && o.DeliveryTime <= firstDeliveryTime.AddMinutes(30))
                .ToList();
        }

        public static void WriteOrdersToFile(List<DeliveryOrder> orders, string filePath)
        {
            try
            {
                using var writer = new StreamWriter(filePath);
                using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                csv.WriteRecords(orders);
                Log.Information($"Результаты фильтрации записаны в файл: {filePath}");
            }
            catch (Exception ex)
            {
                Log.Error($"Ошибка при записи в файл: {ex.Message}");
            }
        }

        private static string GetCommandLineArgument(string[] args, string key)
        {
            var argument = args.FirstOrDefault(arg => arg.StartsWith(key + "="));
            return argument?.Split('=')[1];
        }
    }
}






