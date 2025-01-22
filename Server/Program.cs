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


    static async Task AddUser(string UserData)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseSqlServer(connectionString)
        .Options;
        using (var context = new ApplicationDbContext(options))
        {
            string[] parts = UserData.Split(',');
            string name = parts[0];
            int year = int.Parse(parts[1]);
            decimal weight = decimal.Parse(parts[2], CultureInfo.InvariantCulture);
            decimal height = decimal.Parse(parts[3], CultureInfo.InvariantCulture);

            var user = new User
            {
                Name = name,
                Year = year,
                Weight = weight,
                Height = height,
            };
            context.Users.Add(user);

            context.SaveChanges();
        }
    }

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
        string response = null;
        while (true)
        {
            // Чтение данных от клиента
            bytesRead = networkStream.Read(buffer, 0, buffer.Length);
            if (bytesRead == 0)
            {
                Console.WriteLine("Клиент отключился.");
                break; // Клиент закрыл соединение
            }
            string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);


            string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine("Получено от клиента: " + dataReceived);

            if (receivedData.StartsWith("user:", StringComparison.OrdinalIgnoreCase))
            {
                await AddUser(dataReceived.Substring(5).Trim());
                Console.WriteLine("Ответ сервера: " + response);
            }
            else if (receivedData.StartsWith("product:", StringComparison.OrdinalIgnoreCase))
            {
                // Обрабатываем запрос и дожидаемся результата
                response = await SearchData(dataReceived.Substring(8).Trim());
                Console.WriteLine("Ответ сервера: " + response);
            }

            // Отправляем ответ клиенту
            byte[] responseBytes = Encoding.UTF8.GetBytes(response ?? "Продукт не найден");
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
            context.Database.EnsureCreated();
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
                                string name = productName.Split(' ')[0];
                                string calories = product["nutriments"]?["energy-kcal_100g"]?.ToString() ?? "Нет данных";
                                string protein = product["nutriments"]?["proteins_100g"]?.ToString() ?? "Нет данных";
                                string fat = product["nutriments"]?["fat_100g"]?.ToString() ?? "Нет данных";
                                string carbs = product["nutriments"]?["carbohydrates_100g"]?.ToString() ?? "Нет данных";
                                string categories = product["categories"]?.ToString() ?? "Нет данных";
                                string grams = product?["quantity"]?.ToString().Replace("g", "");
                                grams = grams.Replace("g", "").Replace("mL", "").Trim();

                                decimal calories1 = decimal.TryParse(calories, out var cal) ? cal : 0;
                                decimal protein1 = decimal.TryParse(protein, out var pro) ? pro : 0;
                                decimal fat1 = decimal.TryParse(fat, out var f) ? f : 0;
                                decimal carbs1 = decimal.TryParse(carbs, out var c) ? c : 0;
                                decimal grams1 = decimal.TryParse(grams, out var g) ? g : 0;


                                productInfo = ($"Имя: {name}, Категория: {categories}, Калории: {calories1} ккал на 100г, Белки: {protein1} г, Жиры: {fat1} г, Углеводы: {carbs1} г");
                                //Второе подключение к базе, второе создание контекста на один метод

                                // Убедитесь, что база данных создана
                                context.Database.EnsureCreated();

                                // Создаем объект продукта
                                var products = new Product
                                {
                                    Name = name,
                                    Category = categories,
                                    CaloriesPer100g = calories1 / grams1,
                                    ProteinPer100g = protein1 / grams1,
                                    FatPer100g = fat1 / grams1,
                                    CarbsPer100g = carbs1 / grams1
                                };

                                context.Products.Add(products);


                                string ProductInfo;
                                // Сохраняем изменения в базе данных
                                context.SaveChanges();

                                Console.WriteLine("Продукт успешно добавлен в базу данных.");

                            }
                        }
                        else
                        {
                            SearchData(productName);
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
