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

    static async Task HandleClient(TcpClient client)
    {
        try
        {
            using var networkStream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead;
            string response = null;

            while (true) // Внутренний цикл для обработки сообщений от клиента
            {
                bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    Console.WriteLine("Клиент отключился.");
                    break;
                }

                string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Получено от клиента: " + receivedData);

                if (receivedData.StartsWith("user:", StringComparison.OrdinalIgnoreCase))
                {
                    await AddUser(receivedData.Substring(5).Trim());
                    response = "Пользователь добавлен";
                }
                else if (receivedData.StartsWith("product:", StringComparison.OrdinalIgnoreCase))
                {
                    response = await SearchData(receivedData.Substring(8).Trim());
                }
                else
                {
                    response = "Неизвестная команда";
                }

                // Отправляем ответ клиенту
                byte[] responseBytes = Encoding.UTF8.GetBytes(response ?? "Ошибка обработки");
                await networkStream.WriteAsync(responseBytes, 0, responseBytes.Length);
                Console.WriteLine("Ответ отправлен клиенту.");
            }
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

    static async Task Main()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        using (var context = new ApplicationDbContext(options))
        {
            context.Database.EnsureCreated();
        }

        // Устанавливаем IP-адрес и порт для сервера
        var ipAddress = IPAddress.Any;
        var port = 12345;
        var endpoint = new IPEndPoint(ipAddress, port);

        // Создаем TCP-сервер
        var listener = new TcpListener(endpoint);
        listener.Start();
        Console.WriteLine("Сервер запущен и ожидает подключения клиентов...");

        while (true) // Внешний цикл для поддержки работы сервера
        {
            Console.WriteLine("Ожидаю подключения клиента...");
            var client = await listener.AcceptTcpClientAsync(); // Асинхронное ожидание подключения клиента
            Console.WriteLine("Клиент подключен.");

            // Обрабатываем клиента в отдельной задаче
            _ = Task.Run(() => HandleClient(client));
        }
    }

    public static void CreateFile(string path, string type, string response, double weight, double height, string gender)
    {

        try
        {

            FileInfo fileInfo = new FileInfo(path);
            FileStream fs = fileInfo.Create();
            if (type == "anketa")
            {
                fs.Write(Encoding.UTF8.GetBytes($"{weight}, {height}, {gender}"));
            }
            if (type == "product")
            {
                fs.Write(Encoding.UTF8.GetBytes(response));
            }
            fs.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
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
                productInfo = ($"Имя: {producter.Name}, Категория: {producter.Category}, Калории: {producter.CaloriesPer100g} ккал, Белки: {producter.ProteinPer100g} г, Жиры: {producter.FatPer100g} г, Углеводы: {producter.CarbsPer100g} г");
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


                                productInfo = ($"Имя: {name}, Категория: {categories}, Калории: {calories1}, Белки: {protein1} г, Жиры: {fat1} г, Углеводы: {carbs1} г");
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
