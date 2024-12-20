using System;
using System.Collections.Generic;
using System.Net.Sockets;

class Menu
{
    private static List<string> menuOptions = new List<string>
    {
        "Create Room",
        "Get All Rooms",
        "Join Room",
        "Start Game",
        "Play Game",
        "Exit"
    };

    public static void ShowMenu(NetworkStream stream)
    {
        int selectedIndex = 0;

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Hangman Menu ===");

            for (int i = 0; i < menuOptions.Count; i++)
            {
                if (i == selectedIndex)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"> {menuOptions[i]}");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine($"  {menuOptions[i]}");
                }
            }

            var key = Console.ReadKey(intercept: true).Key;

            switch (key)
            {
                case ConsoleKey.UpArrow:
                    selectedIndex = (selectedIndex - 1 + menuOptions.Count) % menuOptions.Count;
                    break;
                case ConsoleKey.DownArrow:
                    selectedIndex = (selectedIndex + 1) % menuOptions.Count;
                    break;
                case ConsoleKey.Enter:
                    HandleOption(stream, selectedIndex);
                    break;
            }
        }
    }

    private static void HandleOption(NetworkStream stream, int selectedIndex)
    {
        switch (selectedIndex)
        {
            case 0:
                HangmanClient.SimulateCreateRoom(stream);
                break;
            case 1:
                HangmanClient.SimulateGetAllRooms(stream);
                break;
            case 2:
                HangmanClient.SimulateJoinRoom(stream);
                break;
            case 3:
                HangmanClient.SimulateStartGame(stream);
                break;
            case 4:
                HangmanClient.SimulateGamePlay(stream);
                break;
            case 5:
                Console.WriteLine("Exiting...");
                Environment.Exit(0);
                break;
        }

        Console.WriteLine("Press any key to return to the menu...");
        Console.ReadKey();
    }
}
