using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

class TestClient
{
    private const string ServerAddress = "127.0.0.1"; // IP-адрес сервера
    private const int ServerPort = 8001; // Порт сервера

    static void Main(string[] args)
    {
        try
        {
            using (TcpClient client = new TcpClient(ServerAddress, ServerPort))
            using (NetworkStream stream = client.GetStream())
            {
                Console.WriteLine("Connected to server.");

                // 1. Отправляем имя игрока
                var setNameRequest = new SetNameDTO
                {
                    Command = "SET_NAME",
                    Name = "TestPlayer"
                };
                SendMessage(stream, setNameRequest);
                var serverResponse = ReadMessage(stream);
                Console.WriteLine($"Server: {serverResponse}");

                // 2. Создаём комнату
                var createRoomRequest = new CreateRoomDTO
                {
                    Command = "CREATE_ROOM",
                    RoomID = "TestRoom",
                    Password = "12345"
                };
                SendMessage(stream, createRoomRequest);
                serverResponse = ReadMessage(stream);
                Console.WriteLine($"Server: {serverResponse}");

                // 3. Начинаем игру
                var startGameRequest = new StartGameDTO
                {
                    RoomID = "TestRoom" // Указываем идентификатор комнаты
                };

                // Отправка команды на сервер
                SendMessage(stream, startGameRequest);
                serverResponse = ReadMessage(stream);
                Console.WriteLine($"Server: {serverResponse}");

                // 4. Получаем состояние игры
                var getGameStateRequest = new GetGameStateDTO
                {
                    Command = "GET_GAME_STATE",
                    RoomID = "TestRoom"
                };
                SendMessage(stream, getGameStateRequest);
                serverResponse = ReadMessage(stream);
                Console.WriteLine($"Server: {serverResponse}");

                // 3. Отправляем угадывание буквы
                Console.WriteLine("Enter a letter to guess:");
                string letter = Console.ReadLine()?.Trim();

                if (!IsValidLetter(letter))
                {
                    Console.WriteLine("Invalid input. Please enter a single letter.");
                    return;
                }

                var guessLetterRequest = new GuessLetterDTO
                {
                    Command = "GUESS_LETTER",
                    RoomID = "TestRoom",
                    PlayerName = "TestPlayer",
                    Letter = letter
                };
                SendMessage(stream, guessLetterRequest);
                serverResponse = ReadMessage(stream);
                Console.WriteLine($"Server: {serverResponse}");

                // Обработка ответа
                var guessResponse = JsonSerializer.Deserialize<Response<GuessResponse>>(serverResponse);
                if (guessResponse.Status == "error")
                {
                    Console.WriteLine($"Error: {guessResponse.Error.Message}");
                }
                else
                {
                    Console.WriteLine($"Correct: {guessResponse.Data.IsCorrect}");
                    Console.WriteLine($"Feedback: {guessResponse.Data.Feedback}");
                    Console.WriteLine($"Game Over: {guessResponse.Data.GameOver}");
                }
            }
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"Socket error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"General error: {ex.Message}");
        }
    }

    static void SendMessage(NetworkStream stream, object message)
    {
        string jsonMessage = JsonSerializer.Serialize(message);
        byte[] data = Encoding.UTF8.GetBytes(jsonMessage + "\n");
        stream.Write(data, 0, data.Length);
        Console.WriteLine($"Sent: {jsonMessage}");
    }

    static string ReadMessage(NetworkStream stream)
    {
        byte[] buffer = new byte[1024];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        return Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
    }

    static bool IsValidLetter(string input)
    {
        return !string.IsNullOrWhiteSpace(input) && input.Length == 1 && char.IsLetter(input[0]);
    }

    class SetNameDTO
    {
        [JsonPropertyName("command")]
        public string Command { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    class CreateRoomDTO
    {
        [JsonPropertyName("command")]
        public string Command { get; set; }

        [JsonPropertyName("room_id")]
        public string RoomID { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }
    }


    class StartGameDTO
    {
        [JsonPropertyName("command")]
        public string Command { get; set; } = "START_GAME"; // Команда запуска игры

        [JsonPropertyName("room_id")]
        public string RoomID { get; set; } // Идентификатор комнаты, где игра должна быть запущена
    }

    class GetGameStateDTO
    {
        [JsonPropertyName("command")]
        public string Command { get; set; }

        [JsonPropertyName("room_id")]
        public string RoomID { get; set; }
    }

    class GuessLetterDTO
    {
        [JsonPropertyName("command")]
        public string Command { get; set; }

        [JsonPropertyName("room_id")]
        public string RoomID { get; set; }

        [JsonPropertyName("player_name")]
        public string PlayerName { get; set; }

        [JsonPropertyName("letter")]
        public string Letter { get; set; }
    }

    class Response<T>
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("data")]
        public T Data { get; set; }

        [JsonPropertyName("error")]
        public ErrorDTO Error { get; set; }
    }

    class ErrorDTO
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    class GuessResponse
    {
        [JsonPropertyName("is_correct")]
        public bool IsCorrect { get; set; }

        [JsonPropertyName("feedback")]
        public string Feedback { get; set; }

        [JsonPropertyName("game_over")]
        public bool GameOver { get; set; }
    }
}
