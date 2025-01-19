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
    static async Task Main(string[] args)
    {
        string apiUrl = "https://world.openfoodfacts.org/cgi/search.pl";
        string query = "банан"; // Запрос для поиска яблок
        int pageSize = 3;      // Количество результатов на странице

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

                        Console.WriteLine($"Название: {name}");
                        Console.WriteLine($"Калории на 100 г: {calories}");
                        Console.WriteLine($"Белки на 100 г: {protein}");
                        Console.WriteLine($"Жиры на 100 г: {fat}");
                        Console.WriteLine($"Углеводы на 100 г: {carbs}");
                        Console.WriteLine(new string('-', 30));
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