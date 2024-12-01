using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace HangmanClient.Infrastructure
{
    /// <summary>
    /// Класс для работы с TCP-сервером, включая отправку и получение JSON-сообщений.
    /// </summary>
    public class TcpClientHandler : IDisposable
    {
        private readonly TcpClient _client;
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;

        /// <summary>
        /// Создает экземпляр TCP-клиента для подключения к серверу.
        /// </summary>
        /// <param name="address">IP-адрес сервера.</param>
        /// <param name="port">Порт сервера.</param>
        public TcpClientHandler(string address, int port)
        {
            _client = new TcpClient(address, port);
            _reader = new StreamReader(_client.GetStream(), Encoding.UTF8);
            _writer = new StreamWriter(_client.GetStream(), Encoding.UTF8) { AutoFlush = true };
        }

        /// <summary>
        /// Отправляет объект в формате JSON на сервер.
        /// </summary>
        /// <param name="message">Объект для отправки.</param>
        public void SendMessage(object message)
        {
            try
            {
                string jsonMessage = JsonSerializer.Serialize(message);
                _writer.WriteLine(jsonMessage);
                Console.WriteLine($"Sent: {jsonMessage}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Получает JSON-сообщение от сервера.
        /// </summary>
        /// <returns>Ответ сервера в виде строки.</returns>
        public string ReceiveMessage()
        {
            try
            {
                string response = _reader.ReadLine();
                Console.WriteLine($"Received: {response}");
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving message: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Десериализует полученное JSON-сообщение в указанный тип.
        /// </summary>
        /// <typeparam name="T">Тип объекта для десериализации.</typeparam>
        /// <param name="jsonMessage">JSON-сообщение.</param>
        /// <returns>Десериализованный объект.</returns>
        public T DeserializeMessage<T>(string jsonMessage)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(jsonMessage);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error deserializing message: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Закрывает соединение с сервером.
        /// </summary>
        public void Close()
        {
            try
            {
                _reader.Close();
                _writer.Close();
                _client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error closing connection: {ex.Message}");
            }
        }

        /// <summary>
        /// Реализация IDisposable для автоматического освобождения ресурсов.
        /// </summary>
        public void Dispose()
        {
            Close();
        }
    }
}
