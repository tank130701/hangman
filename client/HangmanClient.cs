using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text.Json;
using Google.Protobuf;
using Tcp; // Пространство имён для Protobuf

class HangmanClient
{
    public static void SimulateCreateRoom(NetworkStream stream)
    {
        var createRoomRequest = new CreateRoomRequest
        {
            PlayerUsername = "TestPlayer",
            RoomID = "TestRoom",
            Password = "12345",
            Difficulty = "medium",
            Category = "животные"
        };
        var createRoomPayload = SerializeToJson(createRoomRequest);
        SendMessage(stream, "CREATE_ROOM", createRoomPayload);

        var response = ReadMessage<ServerResponse>(stream);
        Console.WriteLine($"Server Response (Create Room): {response.Message}");
    }

    public static void SimulateGetAllRooms(NetworkStream stream)
    {
        var getAllRoomsRequest = new { Command = "GET_ALL_ROOMS" };
        var getAllRoomsPayload = SerializeToJson(getAllRoomsRequest);
        SendMessage(stream, "GET_ALL_ROOMS", getAllRoomsPayload);

        var response = ReadMessage<ServerResponse>(stream);
        var getAllRoomsResponse = DeserializePayload<GetAllRoomsResponse>(response.Payload);

        Console.WriteLine("Available Rooms:");
        foreach (var room in getAllRoomsResponse.Rooms)
        {
            Console.WriteLine($"Room ID: {room.Id}, Owner: {room.Owner}, Players: {room.PlayersCount}/{room.MaxPlayers}, LastActivity: {room.LastActivity}");
        }
    }

    public static void SimulateJoinRoom(NetworkStream stream)
    {
        var joinRoomRequest = new JoinRoomRequest
        {
            PlayerUsername = "TestPlayer",
            RoomID = "TestRoom",
            Password = "12345",
        };
        var joinRoomPayload = SerializeToJson(joinRoomRequest);
        SendMessage(stream, "JOIN_ROOM", joinRoomPayload);

        var response = ReadMessage<ServerResponse>(stream);
        Console.WriteLine($"Server Response (Join Room): {response.Message}");
    }

    public static void SimulateStartGame(NetworkStream stream)
    {
        var startGameRequest = new StartGameRequest
        {
            PlayerUsername = "TestPlayer",
            RoomID = "TestRoom",
            Password = "12345",
        };
        var startGamePayload = SerializeToJson(startGameRequest);
        SendMessage(stream, "START_GAME", startGamePayload);

        var response = ReadMessage<ServerResponse>(stream);
        Console.WriteLine($"Server Response (Start Game): {response.Message}");
    }

    private static Dictionary<string, int> playerAttempts = new Dictionary<string, int>(); // Прогресс игроков

public static void SimulateGamePlay(NetworkStream stream)
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

    HashSet<char> availableLetters = new HashSet<char>("АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ".ToLowerInvariant().ToCharArray());
    HashSet<char> usedLetters = new HashSet<char>();

    bool gameOver = false;

    while (!gameOver)
    {
        Console.Clear();

        // Запрос состояния игры
        var getGameStateRequest = new GetGameStateRequest
        {
            PlayerUsername = "TestPlayer",
            RoomID = "TestRoom"
        };
        var gameStatePayload = SerializeToJson(getGameStateRequest);
        SendMessage(stream, "GET_GAME_STATE", gameStatePayload);

        var gameStateResponse = ReadMessage<ServerResponse>(stream);
        var gameStateData = DeserializePayload<RoomGameStateResponse>(gameStateResponse.Payload);

        // Обновляем прогресс игроков
        foreach (var player in gameStateData.Players)
        {
            playerAttempts[player.Key] = player.Value.AttemptsLeft;
        }

        // Отображение состояния игроков и их виселиц
        Console.WriteLine("=== Game State ===");
        foreach (var player in gameStateData.Players)
        {
            Console.WriteLine($"Player: {player.Key}");
            Console.WriteLine(hangmanStages[7 - playerAttempts[player.Key]]);
            Console.WriteLine($"Word Progress: {player.Value.WordProgress}");
            Console.WriteLine($"Attempts Left: {player.Value.AttemptsLeft}");
            Console.WriteLine();
        }

        // // Проверка на окончание игры
        // if (gameStateData.Players.Values.All(p => p.IsGameOver))
        // {
        //     Console.WriteLine("Game Over!");
        //     gameOver = true;
        //     break;
        // }

        // Угадывание буквы
        Console.Write("Enter a letter to guess: ");
        string input = Console.ReadLine()?.Trim().ToLowerInvariant();

        // Проверка на корректность ввода
        if (string.IsNullOrWhiteSpace(input) || input.Length != 1 || !char.IsLetter(input[0]))
        {
            Console.WriteLine("Invalid input. Please enter a single Latin letter.");
            Console.ReadKey();
            continue;
        }

        char letter = input[0];

        // Проверка, была ли буква использована
        if (usedLetters.Contains(letter))
        {
            Console.WriteLine("You've already guessed that letter. Try again.");
            Console.ReadKey();
            continue;
        }

        usedLetters.Add(letter);

        // Отправляем запрос на угадывание
        var guessLetterRequest = new GuessLetterRequest
        {
            PlayerUsername = "TestPlayer",
            RoomID = "TestRoom",
            Letter = letter.ToString(),
            Password = "12345"
        };

        var guessLetterPayload = SerializeToJson(guessLetterRequest);
        SendMessage(stream, "GUESS_LETTER", guessLetterPayload);

        var guessLetterResponse = ReadMessage<ServerResponse>(stream);
        var guessLetterData = DeserializePayload<GuessLetterResponse>(guessLetterResponse.Payload);

        // Обратная связь по угадыванию
        Console.WriteLine($"Guess Feedback: {guessLetterData.Feedback}");
        Console.WriteLine($"Correct: {guessLetterData.IsCorrect}");
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    Console.WriteLine("Game Over! Thanks for playing.");
}


    private static T DeserializePayload<T>(Google.Protobuf.ByteString payload)
    {
        return JsonSerializer.Deserialize<T>(payload.ToStringUtf8());
    }

    private static void SendMessage(NetworkStream stream, string command, string payload)
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

    private static T ReadMessage<T>(NetworkStream stream) where T : IMessage<T>, new()
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

    private static string SerializeToJson<T>(T obj)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
        return JsonSerializer.Serialize(obj, options);
    }
}
