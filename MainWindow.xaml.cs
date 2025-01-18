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

namespace GoupExam
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string serverAddress = "127.0.0.1";
        private const int serverPort = 12345;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnGetProductClick(object sender, RoutedEventArgs e)
        {
            SendRequest(QueryTextBox.Text);
        }

        private void OnGetDishClick(object sender, RoutedEventArgs e)
        {
            SendRequest(QueryTextBox.Text);
        }

        private void SendRequest(string query)
        {
            try
            {
                using (TcpClient client = new TcpClient(serverAddress, serverPort))
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] data = Encoding.UTF8.GetBytes(query);
                    stream.Write(data, 0, data.Length);

                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    ResultTextBox.Text = response;
                }
            }
            catch (Exception ex)
            {
                ResultTextBox.Text = "Ошибка: " + ex.Message;
            }
        }
    }
}