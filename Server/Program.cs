using Microsoft.EntityFrameworkCore;
using Server.Models;
using System.Net.Sockets;
using System.Net;
using System.Text;
using GoupExam;



namespace Server
{
    internal class Program
    {
        public static string pr = QueueManedger.key_1;
        private static readonly string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=Calories;Trusted_Connection=True;TrustServerCertificate=True;";
        private static readonly int port = 12345;
        private static readonly string serverAddress = "127.0.0.1";

        async static Task Main(string[] args)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                           .UseSqlServer(connectionString)
                           .Options;

            using var context = new ApplicationDbContext(options);

            // Автоматическое создание базы данных, если её нет
            await context.Database.EnsureCreatedAsync();

            // Заполнение базы данных начальными данными
            await SeedData(context);

            TcpListener server = new TcpListener(IPAddress.Parse(serverAddress), port);
            server.Start();
            Console.WriteLine("Сервер запущен. Ожидание подключений...");

            while (true)
            {
                var client = await server.AcceptTcpClientAsync();
                Console.WriteLine("Подключение клиента установлено.");

                // Обработка подключения клиента в отдельном потоке
                ThreadPool.QueueUserWorkItem(HandleClient, client);
            }
        }

        private static async void HandleClient(object clientObj)
        {
            var client = (TcpClient)clientObj;
            var stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead;

            try
            {
                // Чтение запроса клиента
                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Запрос от клиента: " + request);

                // Получение данных по продукту или блюду
                string response = await GetProductOrDishDetails(request);

                // Отправка ответа клиенту
                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
            }
            finally
            {
                client.Close();
            }
        }

        // Метод для получения данных по продукту или блюду из базы данных
        private static async Task<string> GetProductOrDishDetails(string name)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                           .UseSqlServer(connectionString)
                           .Options;

            using var context = new ApplicationDbContext(options);

            // Преобразуем строку в нижний регистр для сравнения
            var product = await context.Products
                .Where(p => p.Name.ToLower() == name.ToLower())
                .FirstOrDefaultAsync();

            var dish = await context.Dishes
                .Where(d => d.Name.ToLower() == name.ToLower())
                .FirstOrDefaultAsync();

            if (product != null)
            {
                return $"Продукт: {product.Name}\nКатегория: {product.Category}\nКалории (на 100г): {product.CaloriesPer100g}\n" +
                       $"Белки (на 100г): {product.ProteinPer100g}\nЖиры (на 100г): {product.FatPer100g}\nУглеводы (на 100г): {product.CarbsPer100g}";
            }
            else if (dish != null)
            {
                return $"Блюдо: {dish.Name}\nКалории (на порцию): {dish.Calories}\nБелки (на порцию): {dish.Protein}\n" +
                       $"Жиры (на порцию): {dish.Fat}\nУглеводы (на порцию): {dish.Carbs}";
            }
            else
            {
                return "Продукт или блюдо не найдено.";
            }
        }


        // Метод для заполнения базы данных начальными данными
        private static async Task SeedData(ApplicationDbContext context)
        {
            if (!context.Products.Any())
            {
                var initialProducts = new List<Product>
                {
                    new Product
                    {
                        Name = "Яблоко",
                        Category = "Фрукты",
                        CaloriesPer100g = 52,
                        ProteinPer100g = 0.3m,
                        FatPer100g = 0.2m,
                        CarbsPer100g = 14
                    },
                    new Product
                    {
                        Name = "Куриное филе",
                        Category = "Мясо",
                        CaloriesPer100g = 165,
                        ProteinPer100g = 31,
                        FatPer100g = 3.6m,
                        CarbsPer100g = 0
                    }
                };

                context.Products.AddRange(initialProducts);
                await context.SaveChangesAsync();

                Console.WriteLine("Таблица Products была успешно заполнена.");
            }
            else
            {
                Console.WriteLine("Таблица Products уже содержит данные.");
            }

            if (!context.Dishes.Any())
            {
                var initialDishes = new List<Dish>
                {
                    new Dish
                    {
                        Name = "Овощной салат",
                        Calories = 150,
                        Protein = 3,
                        Fat = 10,
                        Carbs = 20
                    },
                    new Dish
                    {
                        Name = "Суп-пюре из тыквы",
                        Calories = 200,
                        Protein = 5,
                        Fat = 8,
                        Carbs = 30
                    }
                };

                context.Dishes.AddRange(initialDishes);
                await context.SaveChangesAsync();

                Console.WriteLine("Таблица Dishes была успешно заполнена.");
            }
            else
            {
                Console.WriteLine("Таблица Dishes уже содержит данные.");
            }
        }
        //метод разбирающий строку параметров,пришедшую от клиента

        private static async Task SeparatingParameters(string str,string pr)
        {
            for(int i = 0; i < str.Length; i++)
            {
                if (str[i] == pr[i])
            }
        }
    }
}