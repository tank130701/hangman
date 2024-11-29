using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

class HangmanClient
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

                // 1. Создание комнаты
                var createRoomRequest = new CreateRoomRequest
                {
                    Command = "CREATE_ROOM",
                    PlayerUsername = "TestPlayer",
                    RoomID = "TestRoom",
                    Password = "12345"
                };
                SendMessage(stream, createRoomRequest);
                Console.WriteLine($"Server: {ReadMessage(stream)}");

                // 3. Запуск игры
                var startGameRequest = new StartGameRequest
                {
                    Command = "START_GAME",
                    PlayerUsername = "TestPlayer",
                    RoomID = "TestRoom"
                };
                SendMessage(stream, startGameRequest);
                Console.WriteLine($"Server: {ReadMessage(stream)}");

                // 4. Игровой процесс
                bool gameOver = false;
                while (!gameOver)
                {
                    // Получение состояния игры
                    var getGameStateRequest = new GetGameStateRequest
                    {
                        Command = "GET_GAME_STATE",
                        PlayerUsername = "TestPlayer",
                        RoomID = "TestRoom"
                    };
                    SendMessage(stream, getGameStateRequest);

                    // Обрабатываем ответ
                    string gameStateResponse = ReadMessage(stream);
                    Console.WriteLine($"Server: {ReadMessage(stream)}");
                    // var gameState = DeserializeResponse<Response<string>>(gameStateResponse);
                    // if (gameState.StatusCode != 2000)
                    // {
                    //     Console.WriteLine($"Error: {gameState.Error.Message}");
                    //     continue;
                    // }
                    // Console.WriteLine($"Game State: {gameState.Data}");

                    // Запрос буквы
                    Console.Write("Enter a letter to guess: ");
                    string letter = Console.ReadLine()?.Trim();

                    if (!IsValidLetter(letter))
                    {
                        Console.WriteLine("Invalid input. Please enter a single letter.");
                        continue;
                    }

                    // Угадывание буквы
                    var guessLetterRequest = new GuessLetterRequest
                    {
                        Command = "GUESS_LETTER",
                        PlayerUsername = "TestPlayer",
                        RoomID = "TestRoom",
                        PlayerName = "TestPlayer",
                        Letter = letter
                    };
                    SendMessage(stream, guessLetterRequest);

                    // Обрабатываем ответ
                    string guessResponseJson = ReadMessage(stream);
                    Console.WriteLine($"Server: {ReadMessage(stream)}");
                    // var guessResponse = DeserializeResponse<Response<GuessLetterResponse>>(guessResponseJson);

                    // if (guessResponse.StatusCode != 2000)
                    // {
                    //     Console.WriteLine($"Error: {guessResponse.Error.Message}");
                    //     continue;
                    // }

                    // var guessData = guessResponse.Data;
                    // Console.WriteLine($"Correct: {guessData.IsCorrect}");
                    // Console.WriteLine($"Feedback: {guessData.Feedback}");
                    // gameOver = guessData.GameOver;
                }

                Console.WriteLine("Game over!");
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
        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
        return message;
    }

    static T DeserializeResponse<T>(string jsonResponse)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(jsonResponse);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error deserializing response: {ex.Message}");
            throw;
        }
    }

    static bool IsValidLetter(string input)
    {
        return !string.IsNullOrWhiteSpace(input) && input.Length == 1 && char.IsLetter(input[0]);
    }
}
