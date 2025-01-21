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
using TopNetwork.RequestResponse;

namespace GoupExam
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string ServerAddress = "127.0.0.1";  // Адрес сервера

        public MainWindow()
        {

            InitializeComponent();
        }

        // Обработчик нажатия на кнопку "Получить данные о продукте"
        private async void OnGetProductClick(object sender, RoutedEventArgs e)
        {
            string query = InputTextBox.Text;

            if (string.IsNullOrWhiteSpace(query))
            {
                OutputTextBox.Text = "Введите название продукта.";
                return;
            }

            string response = await GetResponseFromServer(query);
            OutputTextBox.Text = response;
        }

        // Обработчик нажатия на кнопку "Получить данные о блюде"
        private async void OnGetDishClick(object sender, RoutedEventArgs e)
        {
            string query = InputTextBox.Text;

            if (string.IsNullOrWhiteSpace(query))
            {
                OutputTextBox.Text = "Введите название блюда.";
                return;
            }

            string response = await GetResponseFromServer(query);
            OutputTextBox.Text = response;
        }

        // Общий метод для отправки запроса на сервер и получения ответа
        private async Task<string> GetResponseFromServer(string query)
        {

        }
    }
}