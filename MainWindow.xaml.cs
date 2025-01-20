using System.Net.Sockets;
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
//using Server.Models;  


namespace GoupExam
{
    public partial class MainWindow : Window
    {
        private const string serverAddress = "127.0.0.1";
        private const int serverPort = 12345;
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

            var nameTextBox = new TextBox { Text = "Имя", Margin = new Thickness(0, 5, 0, 5) };
            nameTextBox.GotFocus += (s, e) =>
            {
                if (nameTextBox.Text == "Имя") nameTextBox.Text = "";
            };
            nameTextBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(nameTextBox.Text)) nameTextBox.Text = "Имя";
            };

            var heightTextBox = new TextBox { Text = "Рост (см)", Margin = new Thickness(0, 5, 0, 5) };
            heightTextBox.GotFocus += (s, e) =>
            {
                if (heightTextBox.Text == "Рост (см)") heightTextBox.Text = "";
            };
            heightTextBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(heightTextBox.Text)) heightTextBox.Text = "Рост (см)";
            };

            var weightTextBox = new TextBox { Text = "Вес (кг)", Margin = new Thickness(0, 5, 0, 5) };
            weightTextBox.GotFocus += (s, e) =>
            {
                if (weightTextBox.Text == "Вес (кг)") weightTextBox.Text = "";
            };
            weightTextBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(weightTextBox.Text)) weightTextBox.Text = "Вес (кг)";
            };

            var genderComboBox = new ComboBox { Margin = new Thickness(0, 5, 0, 5) };
            genderComboBox.Items.Add("Мужской");
            genderComboBox.Items.Add("Женский");

            var bodyTypeComboBox = new ComboBox { Margin = new Thickness(0, 5, 0, 5) };
            bodyTypeComboBox.Items.Add("Долихоморфный");
            bodyTypeComboBox.Items.Add("Мезоморфный");
            bodyTypeComboBox.Items.Add("Брахиморфный");


            var submitButton = new Button { Content = "Сохранить", Margin = new Thickness(0, -20, 0, -20) };
            submitButton.Click += (s, args) =>
            {
                if (!string.IsNullOrEmpty(nameTextBox.Text) &&
                    nameTextBox.Text != "Имя" &&
                    !string.IsNullOrEmpty(heightTextBox.Text) &&
                    heightTextBox.Text != "Рост (см)" &&
                    !string.IsNullOrEmpty(weightTextBox.Text) &&
                    weightTextBox.Text != "Вес (кг)" &&
                    genderComboBox.SelectedItem != null &&
                    bodyTypeComboBox.SelectedItem != null)
                {
                    File.WriteAllText("user_data.txt", $"Имя: {nameTextBox.Text}\nРост: {heightTextBox.Text}\nВес: {weightTextBox.Text}\nПол: {genderComboBox.SelectedItem}\nТип телосложения: {bodyTypeComboBox.SelectedItem}");
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
            stackPanel.Children.Add(heightTextBox);
            stackPanel.Children.Add(weightTextBox);
            stackPanel.Children.Add(genderComboBox);
            stackPanel.Children.Add(bodyTypeComboBox);
            stackPanel.Children.Add(submitButton);

            CentralArea.Children.Add(stackPanel);
        }

        // Добавление рекомендаций по диете
        private void ButtonDiet_Click(object sender, RoutedEventArgs e)
        {
            CentralArea.Children.Clear();

            var stackPanel = new StackPanel();
            var radioButton1 = new RadioButton { Content = "Уменьшение веса", Margin = new Thickness(0, 5, 0, 5) };
            var radioButton2 = new RadioButton { Content = "Увеличение веса", Margin = new Thickness(0, 5, 0, 5) };
            var radioButton3 = new RadioButton { Content = "Сохранение веса", Margin = new Thickness(0, 5, 0, 5) };

            var submitButton = new Button { Content = "Подтвердить", Margin = new Thickness(0, 10, 0, 0) };
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
                var weight = double.Parse(userData.FirstOrDefault(line => line.StartsWith("Вес:"))?.Split(':')[1] ?? "0");
                var height = double.Parse(userData.FirstOrDefault(line => line.StartsWith("Рост:"))?.Split(':')[1] ?? "0");
                var gender = userData.FirstOrDefault(line => line.StartsWith("Пол:"))?.Split(':')[1]?.Trim() ?? "";
                var age = 30; // Здесь можно добавить поле для ввода возраста в будущем

                var caloriesInfo = $"Ваш дневной расход калорий для поддержания жизнедеятельности: {CalorieCalculator.Calculate_CaloriesConsupshion(age, weight, height, gender)} ккал/день\n" +
                                   $"Потребность в калориях с учетом активности: {CalorieCalculator.Calculate_CaloriesNeed(CalorieCalculator.Calculate_CaloriesConsupshion(age, weight, height, gender), 1.2)} ккал/день\n" +
                                   $"Ваша дневная норма калорий ({goal}): {CalorieCalculator.Calculate_CaloriesNorm(CalorieCalculator.Calculate_CaloriesNeed(CalorieCalculator.Calculate_CaloriesConsupshion(age, weight, height, gender), 1.2), goal)} ккал/день";

                CentralArea.Children.Clear();
                CentralArea.Children.Add(new TextBlock { Text = caloriesInfo, FontSize = 16, TextWrapping = TextWrapping.Wrap });
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

            //var stackPanel = new StackPanel();

            //// Поле для ввода названия продукта
            //var searchBox = new TextBox { Text = "Введите название продукта", Margin = new Thickness(0, 5, 0, 5) };
            //var searchButton = new Button { Content = "Найти", Margin = new Thickness(0, 5, 0, 5) };
            //var productList = new ListBox { Margin = new Thickness(0, 5, 0, 5) };

            //searchButton.Click += (s, args) =>
            //{
            //    productList.Items.Clear();

            //    // Здесь выполняется поиск продуктов по базе данных
            //    var searchQuery = searchBox.Text;
            //    var products = GetProductsFromDatabase(searchQuery);

            //    if (products.Any())
            //    {
            //        foreach (var product in products)
            //        {
            //            var button = new Button { Content = $"{product.Name} ({product.CaloriesPer100g} ккал на 100г)", Margin = new Thickness(0, 5, 0, 5) };
            //            button.Click += (s, args) =>
            //            {
            //                ProductList.Items.Add($"{product.Name} - {product.CaloriesPer100g} ккал на 100г");
            //                totalCalories += (int)product.CaloriesPer100g;
            //                TotalCalories.Text = totalCalories.ToString();
            //            };
            //            productList.Items.Add(button);
            //        }
            //    }
            //    else
            //    {
            //        MessageBox.Show("Продукт не найден!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            //    }
            //};

            //stackPanel.Children.Add(searchBox);
            //stackPanel.Children.Add(searchButton);
            //stackPanel.Children.Add(productList);

            //CentralArea.Children.Add(stackPanel);
        }

        // Метод для загрузки данных из базы данных
        //private IEnumerable<Product> GetProductsFromDatabase(string query)
        //{
        //    // Пример данных, замените на реальный запрос к базе данных
        //    var sampleProducts = new List<Product>
        //{
        //    new Product { ProductId = 1, Name = "Морковь", Category = "Овощи", CaloriesPer100g = 35, ProteinPer100g = 0.8m, FatPer100g = 0.1m, CarbsPer100g = 6.7m },
        //    new Product { ProductId = 2, Name = "Гречка", Category = "Крупы", CaloriesPer100g = 329, ProteinPer100g = 12.6m, FatPer100g = 3.3m, CarbsPer100g = 62.1m }
        //};

        //    return sampleProducts.Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
        //}

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