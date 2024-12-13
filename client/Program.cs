using System;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Protobuf;
using Tcp; // Namespace, который генерируется из Protobuf

class HangmanMockProtobufClient
{
    private const string ServerAddress = "127.0.0.1";
    private const int ServerPort = 8001;

    static void Main(string[] args)
    {
        try
        {
            using (TcpClient client = new TcpClient(ServerAddress, ServerPort))
            using (NetworkStream stream = client.GetStream())
            {
                Console.WriteLine("Connected to server.");

                // 1. Создание комнаты
                SimulateCreateRoom(stream);

                // 3. Запрос всех комнат
                SimulateGetAllRooms(stream);

                // 2. Присоединение к комнате
                SimulateJoinRoom(stream);

                // 3. Запуск игры
                SimulateStartGame(stream);

                // 4. Имитация игрового процесса
                SimulateGamePlay(stream);
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

    static void SimulateCreateRoom(NetworkStream stream)
    {
        var createRoomRequest = new CreateRoomRequest
        {
            PlayerUsername = "TestPlayer",
            RoomID = "TestRoom",
            Password = "12345",
            Command = "CREATE_ROOM"
        };
        var createRoomPayload = SerializeToJson(createRoomRequest);
        SendMessage(stream, "CREATE_ROOM", createRoomPayload);

        var response = ReadMessage<ServerResponse>(stream);
        Console.WriteLine($"Server Response (Create Room): {response.Message}");
    }

    static void SimulateGetAllRooms(NetworkStream stream)
    {
        var getAllRoomsRequest = new { Command = "GET_ALL_ROOMS" }; // Простая структура для команды
        var getAllRoomsPayload = SerializeToJson(getAllRoomsRequest);
        SendMessage(stream, "GET_ALL_ROOMS", getAllRoomsPayload);

        var response = ReadMessage<ServerResponse>(stream);

        // Десериализация списка комнат
        var rooms = DeserializePayload<List<RoomDTO>>(response.Payload);

        Console.WriteLine("Available Rooms:");
        foreach (var room in rooms)
        {
            Console.WriteLine($"Room ID: {room.Id}, Owner: {room.Owner}, Players: {room.PlayersCount}/{room.MaxPlayers}, IsOpen: {room.IsOpen}, LastActivity: {room.LastActivity}");
        }
    }

    static void SimulateJoinRoom(NetworkStream stream)
    {
        var joinRoomRequest = new JoinRoomRequest
        {
            PlayerUsername = "TestPlayer",
            RoomID = "TestRoom",
            Password = "12345",
            Command = "JOIN_ROOM"
        };
        var joinRoomPayload = SerializeToJson(joinRoomRequest);
        SendMessage(stream, "JOIN_ROOM", joinRoomPayload);

        var response = ReadMessage<ServerResponse>(stream);
        Console.WriteLine($"Server Response (Join Room): {response.Message}");
    }

    static void SimulateStartGame(NetworkStream stream)
    {
        var startGameRequest = new StartGameRequest
        {
            PlayerUsername = "TestPlayer",
            RoomID = "TestRoom",
            Command = "START_GAME"
        };
        var startGamePayload = SerializeToJson(startGameRequest);
        SendMessage(stream, "START_GAME", startGamePayload);

        var response = ReadMessage<ServerResponse>(stream);
        Console.WriteLine($"Server Response (Start Game): {response.Message}");
    }

    static void SimulateGamePlay(NetworkStream stream)
    {
        string[] hangmanStages = new string[]
        {
        "\n\n\n\n\n\n",
        "\n\n\n\n\n\n_____",
        "\n |\n |\n |\n |\n_|___",
        "_____\n |   |\n |\n |\n |\n_|___",
        "_____\n |   |\n |   O\n |\n |\n_|___",
        "_____\n |   |\n |   O\n |  /|\\\n |\n_|___",
        "_____\n |   |\n |   O\n |  /|\\\n |  / \\\n_|___"
        };

        bool gameOver = false;
        while (!gameOver)
        {
            Console.Clear();

            // Запрос состояния игры
            var getGameStateRequest = new GetGameStateRequest
            {
                PlayerUsername = "TestPlayer",
                RoomID = "TestRoom",
                Command = "GET_GAME_STATE"
            };
            var gameStatePayload = SerializeToJson(getGameStateRequest);
            SendMessage(stream, "GET_GAME_STATE", gameStatePayload);

            var gameStateResponse = ReadMessage<ServerResponse>(stream);

            // Десериализация Payload в GameStateResponse
            var gameStateData = DeserializePayload<GameStateResponse>(gameStateResponse.Payload);

            // Отображение состояния игры
            // Console.WriteLine(hangmanStages[gameStateData.Attempts]);
            // Console.WriteLine($"Word: {gameStateData.Word}");
            // Console.WriteLine($"Attempts: {gameStateData.Attempts}/{gameStateData.MaxAttempts}");

            // if (gameStateData.Attempts >= gameStateData.MaxAttempts)
            // {
            //     Console.WriteLine("Game Over! You've been hanged.");
            //     break;
            // }

            // Угадывание буквы
            Console.Write("Enter a letter to guess: ");
            string letter = Console.ReadLine()?.Trim();

            var guessLetterRequest = new GuessLetterRequest
            {
                PlayerUsername = "TestPlayer",
                RoomID = "TestRoom",
                Letter = letter,
                Command = "GUESS_LETTER"
            };
            var guessLetterPayload = SerializeToJson(guessLetterRequest);
            SendMessage(stream, "GUESS_LETTER", guessLetterPayload);

            var guessLetterResponse = ReadMessage<ServerResponse>(stream);

            // Десериализация GuessLetterResponse
            var guessLetterData = DeserializePayload<GuessLetterResponse>(guessLetterResponse.Payload);
            Console.WriteLine($"Guess Feedback: {guessLetterData.Feedback}");
            Console.WriteLine($"Correct: {guessLetterData.IsCorrect}");

            // Проверяем, завершилась ли игра
            gameOver = guessLetterData.GameOver;

            if (gameOver)
            {
                if (guessLetterData.IsCorrect)
                {
                    Console.WriteLine("Congratulations! You guessed the word.");
                }
                else
                {
                    Console.WriteLine("Game Over! You've been hanged.");
                }
            }
        }

        Console.WriteLine("Game Over! Thanks for playing.");
    }

    static T DeserializePayload<T>(Google.Protobuf.ByteString payload)
    {
        return JsonSerializer.Deserialize<T>(payload.ToStringUtf8());
    }
    static void SendMessage(NetworkStream stream, string command, string payload)
    {
        var clientMessage = new ClientMessage
        {
            Command = command,
            Payload = Google.Protobuf.ByteString.CopyFromUtf8(payload)
        };

        byte[] data = clientMessage.ToByteArray();
        byte[] header = BitConverter.GetBytes(data.Length);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(header);

        stream.Write(header, 0, header.Length);
        stream.Write(data, 0, data.Length);

        Console.WriteLine($"Sent: {command}");
    }

    static T ReadMessage<T>(NetworkStream stream) where T : IMessage<T>, new()
    {
        byte[] header = new byte[4];
        stream.Read(header, 0, header.Length);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(header);
        int messageLength = BitConverter.ToInt32(header, 0);

        byte[] body = new byte[messageLength];
        int totalBytesRead = 0;
        while (totalBytesRead < messageLength)
        {
            totalBytesRead += stream.Read(body, totalBytesRead, messageLength - totalBytesRead);
        }

        var parser = new MessageParser<T>(() => new T());
        return parser.ParseFrom(body);
    }

    static string SerializeToJson<T>(T obj)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
        return JsonSerializer.Serialize(obj, options);
    }
}
