using System;
using System.Text.RegularExpressions;
using client.Domain.Interfaces;
namespace client.Menu
{
    public class ServerAddressValidator
    {
        public string GetValidatedServerAddress()
        {
            string serverAddress;
            while (true)
            {
                Console.Write("Enter the server address (IPv4 format, e.g., 127.0.0.1): ");
                serverAddress = Console.ReadLine() ?? string.Empty;

                // Проверка формата IPv4
                if (Regex.IsMatch(serverAddress, @"^(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\." +
                                                @"(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\." +
                                                @"(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\." +
                                                @"(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])$"))
                {
                    break;
                }

                Console.WriteLine("Invalid server address. Please enter a valid IPv4 address.");
            }
            return serverAddress;
        }
    }

    public class UsernameValidator
    {
        private readonly IGameDriver _gameDriver;
        public UsernameValidator(IGameDriver driver)
        {
            _gameDriver = driver;
        }
        public string GetValidatedUsername()
        {
            string username;
            while (true)
            {
                Console.Write("Enter your username (3-15 characters, alphanumeric): ");
                username = Console.ReadLine() ?? string.Empty;

                // Проверка: от 3 до 15 символов, только буквы и цифры
                if (!string.IsNullOrEmpty(username) && Regex.IsMatch(username, @"^[a-zA-Z0-9]{3,15}$"))
                {
                    // Проверка уникальности имени пользователя
                    var response = _gameDriver.CheckUsername(username);
                    if (!response.IsUnique)
                    {
                        Console.WriteLine("Username is already taken. Please choose another one.");
                        continue;
                    }
                    break;
                }

                Console.WriteLine("Invalid username. Please use 3-15 alphanumeric characters.");
            }
            return username;
        }
    }
}