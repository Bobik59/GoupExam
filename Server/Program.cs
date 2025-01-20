using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Globalization;
using System.Linq;
using Server.Models;
using Microsoft.EntityFrameworkCore;
class Program
{
    private static readonly string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=Calories;Trusted_Connection=True;TrustServerCertificate=True;";

    static async Task Main(string[] args)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
               .UseSqlServer(connectionString)
               .Options;

        using var context = new ApplicationDbContext(options);

        await context.Database.EnsureCreatedAsync();

        string apiUrl = "https://world.openfoodfacts.org/cgi/search.pl";
        string query = "яблоки";
        int pageSize = 1;

        // Параметры запроса
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
}