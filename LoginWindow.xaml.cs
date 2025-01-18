using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GoupExam
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string userName = UserNameTextBox.Text;
            if (string.IsNullOrWhiteSpace(userName))
            {
                MessageBox.Show("Введите имя пользователя.");
                return;
            }

            // Подключаемся к серверу
            using (TcpClient client = new TcpClient("127.0.0.1", 12345))
            {
                var stream = client.GetStream();
                byte[] buffer = Encoding.UTF8.GetBytes(userName);
                await stream.WriteAsync(buffer, 0, buffer.Length);

                byte[] responseBuffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(responseBuffer, 0, responseBuffer.Length);
                string response = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);

                // Обрабатываем ответ от сервера
                if (response == "UserExists")
                {
                    // Если пользователь существует, открываем главное окно
                    MainWindow mainWindow = new MainWindow(userName);
                    mainWindow.Show();
                    this.Close();
                }
                else if (response == "NewUserCreated")
                {
                    // Если пользователь новый, отображаем окно добавления данных
                    UserDetailsWindow userDetailsWindow = new UserDetailsWindow(userName);
                    userDetailsWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Ошибка при подключении к серверу.");
                }
            }
        }
    }
}