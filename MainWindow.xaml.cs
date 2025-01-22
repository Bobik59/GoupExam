using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using Server.DataBase;
using System.Globalization;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Security.Policy;
using static System.Runtime.InteropServices.JavaScript.JSType;
//using Server.Models;  


namespace GoupExam
{
    public partial class MainWindow : Window
    {
        
        private string ipAddress = "127.0.0.1"; // Локальный адрес
        private int port = 12345;
        private int totalCalories = 0;

        public MainWindow()
        {
            InitializeComponent();
            CheckForUserFile();
          
           
        }

        private void CheckForUserFile()
        {
            if (File.Exists("user_data.txt"))
            {
                ButtonProducts.IsEnabled = true;
                ButtonDiet.IsEnabled = true;
            }
        }

        private void ButtonForm_Click(object sender, RoutedEventArgs e)
        {
            CentralArea.Children.Clear();

            var stackPanel = new StackPanel();

            var nameTextBox = new TextBox { Text = "Имя", Margin = new Thickness(0, 5, 0, 5), Foreground = new SolidColorBrush(Color.FromRgb(2, 140, 47)), Background = new SolidColorBrush(Color.FromRgb(178, 225, 0)) };
            nameTextBox.GotFocus += (s, e) =>
            {
                if (nameTextBox.Text == "Имя") nameTextBox.Text = "";
            };
            nameTextBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(nameTextBox.Text)) nameTextBox.Text = "Имя";
            };

            var heightTextBox = new TextBox {Text = "Рост (см)", Margin = new Thickness(0, 5, 0, 5), Foreground = new SolidColorBrush(Color.FromRgb(2, 140, 47)), Background = new SolidColorBrush(Color.FromRgb(178, 225, 0))};
            heightTextBox.GotFocus += (s, e) =>
            {
                if (heightTextBox.Text == "Рост (см)") heightTextBox.Text = "";
            };
            heightTextBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(heightTextBox.Text)) heightTextBox.Text = "Рост (см)";
            };

            var weightTextBox = new TextBox {Text = "Вес (кг)", Margin = new Thickness(0, 5, 0, 5), Foreground = new SolidColorBrush(Color.FromRgb(2, 140, 47)), Background = new SolidColorBrush(Color.FromRgb(178, 225, 0)) };
            weightTextBox.GotFocus += (s, e) =>
            {
                if (weightTextBox.Text == "Вес (кг)") weightTextBox.Text = "";
            };
            weightTextBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(weightTextBox.Text)) weightTextBox.Text = "Вес (кг)";
            };

            var AgeTextBox = new TextBox {Text = "Возраст(год)", Margin = new Thickness(0, 5, 0, 5), Foreground = new SolidColorBrush(Color.FromRgb(2, 140, 47)), Background = new SolidColorBrush(Color.FromRgb(178, 225, 0)) };
            AgeTextBox.GotFocus += (s, e) =>
            {
                if (AgeTextBox.Text == "Возраст(год)") AgeTextBox.Text = "";
            };
            AgeTextBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(AgeTextBox.Text)) AgeTextBox.Text = "Возраст(год)";
            };

            var genderComboBox = new ComboBox { Margin = new Thickness(0, 5, 0, 5)};
            genderComboBox.Items.Add("Мужской");
            genderComboBox.Items.Add("Женский");

            var bodyTypeComboBox = new ComboBox { Margin = new Thickness(0, 5, 0, 5)};
            bodyTypeComboBox.Items.Add("Долихоморфный");
            bodyTypeComboBox.Items.Add("Мезоморфный");
            bodyTypeComboBox.Items.Add("Брахиморфный");

            var submitButton = new Button { Content = "Сохранить", Margin = new Thickness(0, -10, 0, -10) };

            var submitButton = new Button { Content = "Сохранить", Margin = new Thickness(10, 10, 10, -5), Foreground = new SolidColorBrush(Color.FromRgb(2, 140, 47)), Background = new SolidColorBrush(Color.FromRgb(178, 225, 0)) };
            submitButton.Click += (s, args) =>
            {
                if (!string.IsNullOrEmpty(nameTextBox.Text) &&
                    nameTextBox.Text != "Имя" &&
                    !string.IsNullOrEmpty(ageTextBox.Text) &&
                    ageTextBox.Text != "Возраст" &&
                    !string.IsNullOrEmpty(heightTextBox.Text) &&
                    heightTextBox.Text != "Рост (см)" &&
                    !string.IsNullOrEmpty(weightTextBox.Text) &&
                    weightTextBox.Text != "Вес (кг)" &&
                    genderComboBox.SelectedItem != null &&
                    bodyTypeComboBox.SelectedItem != null)
                {
                    File.WriteAllText("user_data.txt", $"Имя: {nameTextBox.Text}\nВозраст: {ageTextBox.Text}\nРост: {heightTextBox.Text}\nВес: {weightTextBox.Text}\nПол: {genderComboBox.SelectedItem}\nТип телосложения: {bodyTypeComboBox.SelectedItem}");

                    // Формируем строку для метода
                    string searchQuery = $"user: {nameTextBox.Text},{ageTextBox.Text},{weightTextBox.Text},{heightTextBox.Text}";

                    // Вызываем метод SearchProductsAsync
                    await SearchProductsAsync(searchQuery);

                    MessageBox.Show("Данные сохранены!");
                    ButtonProducts.IsEnabled = true;
                    ButtonDiet.IsEnabled = true;
                }
                else
                {
                    MessageBox.Show("Заполните все поля анкеты!");
                }
            };

            stackPanel.Children.Add(nameTextBox);
            stackPanel.Children.Add(ageTextBox);
            stackPanel.Children.Add(heightTextBox);
            stackPanel.Children.Add(weightTextBox);
            stackPanel.Children.Add(genderComboBox);
            stackPanel.Children.Add(bodyTypeComboBox);
            stackPanel.Children.Add(submitButton);

            CentralArea.Children.Add(stackPanel);
        }

        public static (string Name, double Number)? ParseData(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            // Разделяем строку на части
            var parts = input.Split(new[] { ' ', ',', ';', '\t', ':' }, StringSplitOptions.RemoveEmptyEntries);

            // Если в строке меньше двух частей, данные некорректны
            if (parts.Length < 2)
            {
                return null;
            }

            // Последняя часть - число
            string numberPart = parts[^1];

            // Всё до последней части - название
            string name = string.Join(" ", parts, 0, parts.Length - 1).Trim();

            if (double.TryParse(numberPart, NumberStyles.Any, CultureInfo.InvariantCulture, out double number))
            {
                return (name, number);
            }

            return null;
        }


        private void ButtonMenu_Click(object sender, RoutedEventArgs e)
        {
            CentralArea.Children.Clear();

            // Создание элементов для интерфейса
            var stackPanel = new StackPanel();
            string InfoProducts = null;
            var textBox = new TextBox { Margin = new Thickness(0, 5, 0, 5), Width = 300, Height = 30, Text = "Введите данные" };
            var addButton = new Button { Content = "Добавить", Margin = new Thickness(0, 10, 0, 10), Width = 100, Height = 30 };
            var listBox = new ListBox { Margin = new Thickness(0, 10, 0, 10), Width = 300, Height = 150 };

            // Логика добавления данных в список
            addButton.Click += async (s, args) =>
            {
                if (!string.IsNullOrWhiteSpace(textBox.Text))
                {
                    var result = ParseData(textBox.Text);
                    string apiUrl = "https://world.openfoodfacts.org/cgi/search.pl";
                    string query = result.Value.Name;
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

                                foreach (var product in data["products"])
                                {
                                    string name = result.Value.Name.Split(' ')[0];
                                    string calories = product["nutriments"]?["energy-kcal_100g"]?.ToString() ?? "Нет данных";
                                    string grams = product?["quantity"]?.ToString().Replace("g", "");
                                    grams = grams.Replace("g", "").Replace("mL", "").Trim();

                                    double calories1 = double.TryParse(calories, out var cal) ? cal : 0;
                                    double grams1 = double.TryParse(grams, out var g) ? g : 0;

                                    double roundedNumber = Math.Round((calories1 / grams1) * result.Value.Number, 2);
                                    InfoProducts = $"{name} {roundedNumber}calloria, {result.Value.Number}gramm ";


                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка: {ex.Message}");
                        }
                    }
                    listBox.Items.Add(InfoProducts);
                    textBox.Clear();
                }
                else
                {
                    MessageBox.Show("Введите данные перед добавлением.");
                }
            };

            // Добавление элементов в StackPanel
            stackPanel.Children.Add(textBox);
            stackPanel.Children.Add(addButton);
            stackPanel.Children.Add(listBox);

            // Добавление StackPanel в центральную область
            CentralArea.Children.Add(stackPanel);
        }



        // Добавление рекомендаций по диете
        private void ButtonDiet_Click(object sender, RoutedEventArgs e)
        {
            CentralArea.Children.Clear();
            //Foreground = "#FF028C2F" Background = "#FFB2E100"
            var stackPanel = new StackPanel();
            var radioButton1 = new RadioButton { Content = "Уменьшение веса", Margin = new Thickness(0, 5, 0, 5), Foreground = new SolidColorBrush(Color.FromRgb(2,140,47)), Background = new SolidColorBrush(Color.FromRgb(178, 225, 0)) };
            var radioButton2 = new RadioButton { Content = "Увеличение веса", Margin = new Thickness(0, 5, 0, 5), Foreground = new SolidColorBrush(Color.FromRgb(2, 140, 47)), Background = new SolidColorBrush(Color.FromRgb(178, 225, 0)) };
            var radioButton3 = new RadioButton {Content = "Сохранение веса", Margin = new Thickness(0, 5, 0, 5), Foreground = new SolidColorBrush(Color.FromRgb(2, 140, 47)), Background = new SolidColorBrush(Color.FromRgb(178, 225, 0)) };

            var submitButton = new Button { Content = "Подтвердить", Margin = new Thickness(0, 10, 0, 0), Foreground = new SolidColorBrush(Color.FromRgb(2, 140, 47)), Background = new SolidColorBrush(Color.FromRgb(178, 225, 0)) };
            submitButton.Click += (s, args) =>
            {
                string selectedOption = null;
                Goal goal;

                if (radioButton1.IsChecked == true)
                {
                    selectedOption = "Уменьшение веса";
                    goal = Goal.MinusWeight;
                }
                else if (radioButton2.IsChecked == true)
                {
                    selectedOption = "Увеличение веса";
                    goal = Goal.PlusWeight;
                }
                else if (radioButton3.IsChecked == true)
                {
                    selectedOption = "Сохранение веса";
                    goal = Goal.StopWeight;
                }
                else
                {
                    MessageBox.Show("Выберите один из вариантов!");
                    return;
                }

                // Загрузка данных пользователя
                var userData = File.ReadAllLines("user_data.txt");
                //var userName = (userData.FirstOrDefault(line => line.StartsWith("Имя:"))?.Split(':')[1] ?? "0");
                var weight = double.Parse(userData.FirstOrDefault(line => line.StartsWith("Вес:"))?.Split(':')[1] ?? "0");
                var height = double.Parse(userData.FirstOrDefault(line => line.StartsWith("Рост:"))?.Split(':')[1] ?? "0");
                var gender = userData.FirstOrDefault(line => line.StartsWith("Пол:"))?.Split(':')[1]?.Trim() ?? "";
                var age = 30; // Здесь можно добавить поле для ввода возраста в будущем

                var caloriesInfo = $"Ваш дневной расход калорий для поддержания жизнедеятельности: {CalorieCalculator.Calculate_CaloriesConsupshion(age, weight, height, gender)} ккал/день\n" +
                                   $"Потребность в калориях с учетом активности: {CalorieCalculator.Calculate_CaloriesNeed(CalorieCalculator.Calculate_CaloriesConsupshion(age, weight, height, gender), 1.2)} ккал/день\n" +
                                   $"Ваша дневная норма калорий ({goal}): {CalorieCalculator.Calculate_CaloriesNorm(CalorieCalculator.Calculate_CaloriesNeed(CalorieCalculator.Calculate_CaloriesConsupshion(age, weight, height, gender), 1.2), goal)} ккал/день";

                CentralArea.Children.Clear();
                CentralArea.Children.Add(new TextBlock { Text = caloriesInfo, FontSize = 16, TextWrapping = TextWrapping.Wrap, Foreground = new SolidColorBrush(Color.FromRgb(2, 140, 47)), Background = new SolidColorBrush(Color.FromRgb(178, 225, 0)) });
            };

            stackPanel.Children.Add(radioButton1);
            stackPanel.Children.Add(radioButton2);
            stackPanel.Children.Add(radioButton3);
            stackPanel.Children.Add(submitButton);

            CentralArea.Children.Add(stackPanel);
        }



        private void ButtonProducts_Click(object sender, RoutedEventArgs e)
        {
            CentralArea.Children.Clear();
            var stackPanel = new StackPanel();
            var searchBox = new TextBox {Text = "", Margin = new Thickness(0, 5, 0, 5), Foreground = new SolidColorBrush(Color.FromRgb(2, 140, 47)), Background = new SolidColorBrush(Color.FromRgb(178, 225, 0)) };
            var searchButton = new Button {Content = "Найти", Margin = new Thickness(0, 5, 0, 5), Foreground = new SolidColorBrush(Color.FromRgb(2, 140, 47)), Background = new SolidColorBrush(Color.FromRgb(178, 225, 0)) };
            var productList = new ListBox { Margin = new Thickness(0, 5, 0, 5), Foreground = new SolidColorBrush(Color.FromRgb(2, 140, 47)), Background = new SolidColorBrush(Color.FromRgb(178, 225, 0)) };
            //// Поле для ввода названия продукта

            searchButton.Click += async (s, args) =>
            {
                productList.Items.Clear();

                // Здесь выполняется поиск продуктов по базе данных
                var searchQuery = searchBox.Text;

                try
                {
                    // Асинхронное взаимодействие с сервером
                    var response = await SearchProductsAsync("product:"+searchQuery);
                    productList.Items.Add(response);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Произошла ошибка: " + ex.Message);
                }
            };

            stackPanel.Children.Add(searchBox);
            stackPanel.Children.Add(searchButton);
            stackPanel.Children.Add(productList);

            CentralArea.Children.Add(stackPanel);
        }
        private async Task<string> SearchProductsAsync(string searchQuery)
        {

            using (var client = new TcpClient())
            {
                await client.ConnectAsync(ipAddress, port); // Асинхронное подключение к серверу

                using (var networkStream = client.GetStream())
                {
                    // Отправляем сообщение серверу
                    byte[] messageBytes = Encoding.UTF8.GetBytes(searchQuery);
                    await networkStream.WriteAsync(messageBytes, 0, messageBytes.Length);

                    Console.WriteLine("Сообщение отправлено.");

                    // Читаем ответ от сервера
                    byte[] buffer = new byte[1024];
                    int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    return response;
                }
            }
        }




        private void RemoveProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ProductList.SelectedItem != null)
            {
                // Получаем выделенный элемент
                var selectedItem = ProductList.SelectedItem.ToString();

                // Извлекаем количество калорий из строки
                var caloriePart = selectedItem.Split('-')[1].Trim();
                int caloriesToRemove = int.Parse(caloriePart.Split(' ')[0]);

                // Уменьшаем общее количество калорий
                totalCalories -= caloriesToRemove;
                TotalCalories.Text = totalCalories.ToString();

                // Удаляем элемент из списка
                ProductList.Items.Remove(selectedItem);
            }
            else
            {
                MessageBox.Show("Выберите продукт для удаления!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}