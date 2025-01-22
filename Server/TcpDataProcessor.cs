using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class TcpDataProcessor
    {
        private const string ServerAddress = "127.0.0.1";  // Адрес сервера (замени на свой)
        private const int Port = 12345;  // Порт для подключения

        // Метод для отправки данных на сервер через TCP
        public async Task SendDataToServer(string input)
        {
            try
            {
                using (var tcpClient = new TcpClient(ServerAddress, Port))
                using (var stream = tcpClient.GetStream())
                {
                    // Кодируем данные в байты
                    byte[] dataToSend = Encoding.UTF8.GetBytes(input);

                    // Отправляем данные на сервер
                    await stream.WriteAsync(dataToSend, 0, dataToSend.Length);

                    Console.WriteLine("Данные успешно отправлены на сервер.");

                    // Ожидаем ответа от сервера
                    byte[] buffer = new byte[1024];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    Console.WriteLine("Ответ от сервера: " + response);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отправке данных: {ex.Message}");
            }
        }
    }
}
