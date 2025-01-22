using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Globalization;
using System.Linq;
using Server.DataBase;
using Microsoft.EntityFrameworkCore;
using System.Net.Sockets;
using System.Net;
using System.Text;
class Program // сервер 
{
    private static readonly string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=Calories;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";

    static async Task Main()
    {
        // Устанавливаем IP-адрес и порт для сервера
        var ipAddress = IPAddress.Any;
        var port = 12345;
        var endpoint = new IPEndPoint(ipAddress, port);

        // Создаем TCP-сервер
        var listener = new TcpListener(endpoint);
        listener.Start();
        Console.WriteLine("Ожидаю подключения клиента...");

        // Ожидаем подключения клиента
        var client = listener.AcceptTcpClient();
        Console.WriteLine("Клиент подключен.");

        // Получаем поток для чтения и записи
        var networkStream = client.GetStream();

        // Цикл обмена сообщениями
        byte[] buffer = new byte[1024];
        int bytesRead;

        while (true)
        {
            // Чтение данных от клиента
            bytesRead = networkStream.Read(buffer, 0, buffer.Length);
            if (bytesRead == 0)
            {
                Console.WriteLine("Клиент отключился.");
                break; // Клиент закрыл соединение
            }

            string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine("Получено от клиента: " + dataReceived);

            // Обрабатываем запрос и дожидаемся результата
            string response = await SearchData(dataReceived);
            Console.WriteLine("Ответ сервера: " + response);

            // Отправляем ответ клиенту
            byte[] responseBytes = Encoding.UTF8.GetBytes(response ?? "Продукт не найден");//
            networkStream.Write(responseBytes, 0, responseBytes.Length);
            Console.WriteLine("Ответ отправлен клиенту.");
        }

        // Закрываем соединение
        client.Close();
        listener.Stop();
    }

    static async Task<string> SearchData(string productName)
    {
        string productInfo = null;
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        using (var context = new ApplicationDbContext(options))
        {
            context.Database.EnsureCreatedAsync();
            var producter = context.Products
                    .FirstOrDefault(p => p.Name.ToLower() == productName.ToLower());

            if (producter != null)
            {
                productInfo = ($"Имя: {producter.Name}, Категория: {producter.Category}, Калории: {producter.CaloriesPer100g} ккал на 100г, Белки: {producter.ProteinPer100g} г, Жиры: {producter.FatPer100g} г, Углеводы: {producter.CarbsPer100g} г");
            }
            else
            {
                string apiUrl = "https://world.openfoodfacts.org/cgi/search.pl";
                string query = productName;
                int pageSize = 1;

                string url = $"{apiUrl}?search_terms={query}&page_size={pageSize}&json=true";

                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        HttpResponseMessage response = await client.GetAsync(url);

                        if (response.IsSuccessStatusCode)
                        {
                            string jsonResponse = await response.Content.ReadAsStringAsync();

                            // Парсинг JSON-ответа
                            JObject data = JObject.Parse(jsonResponse);

                            Console.WriteLine("Результаты поиска продуктов:");
                            foreach (var product in data["products"])
                            {
                                string name = product["product_name"]?.ToString() ?? "Без названия";
                                string calories = product["nutriments"]?["energy-kcal_100g"]?.ToString() ?? "Нет данных";
                                string protein = product["nutriments"]?["proteins_100g"]?.ToString() ?? "Нет данных";
                                string fat = product["nutriments"]?["fat_100g"]?.ToString() ?? "Нет данных";
                                string carbs = product["nutriments"]?["carbohydrates_100g"]?.ToString() ?? "Нет данных";
                                string categories = product["categories"]?.ToString() ?? "Нет данных";

                                decimal calories1 = decimal.TryParse(calories, out var cal) ? cal : 0;
                                decimal protein1 = decimal.TryParse(protein, out var pro) ? pro : 0;
                                decimal fat1 = decimal.TryParse(fat, out var f) ? f : 0;
                                decimal carbs1 = decimal.TryParse(carbs, out var c) ? c : 0;

                                productInfo = ($"Имя: {name}, Категория: {categories}, Калории: {calories1} ккал на 100г, Белки: {protein1} г, Жиры: {fat1} г, Углеводы: {carbs1} г");
                                using (var dbContext = new ApplicationDbContext(options))
                                {
                                    // Убедитесь, что база данных создана
                                    dbContext.Database.EnsureCreated();

                                    // Создаем объект продукта
                                    var products = new Product
                                    {
                                        Name = name,
                                        Category = categories,
                                        CaloriesPer100g = calories1,
                                        ProteinPer100g = protein1,
                                        FatPer100g = fat1,
                                        CarbsPer100g = carbs1
                                    };

                                    dbContext.Products.Add(products);


                                    string ProductInfo;
                                    // Сохраняем изменения в базе данных
                                    dbContext.SaveChanges();

                                    Console.WriteLine("Продукт успешно добавлен в базу данных.");
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Ошибка запроса: {response.StatusCode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка: {ex.Message}");
                    }
                }
            }
            return productInfo;
        }
    }
}