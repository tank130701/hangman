using client.Application;
using client.Infrastructure;
using client.Menu;
using client.Presentation;
using NLog;
using System.Diagnostics;

namespace client
{

    class Program
    {
        // Создание окна для логов
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
      
            // Настройка NLog
            LogManager.LoadConfiguration("NLog.config");
            // Пример логирования
            logger.Info("Log console started.");

            Console.Clear();
            // Создаем экземпляр класса InputHandler
            InputHandler inputHandler = new InputHandler();

            // Получаем имя пользователя и адрес сервера
            string username = inputHandler.GetValidatedUsername();
            // string serverAddress = inputHandler.GetValidatedServerAddress();

            // string username = "test";
            string serverAddress = "127.0.0.1";


            Console.WriteLine($"\nWelcome, {username}! Connecting to server at {serverAddress}...");
            const int serverPort = 8001; // Порт сервера
            var tcpClient = new TcpClientHandler(serverAddress, serverPort);

            // tcpClient.Connect();
            var gameService = new GameDriver(tcpClient, username);
            var gameUi = new GameUI(gameService);

            string headerText = "  _   _                                         " +
                Environment.NewLine + " | | | |                                        " +
                Environment.NewLine + " | |_| | __ _ _ __   __ _ _ __ ___   __ _ _ __ " +
                Environment.NewLine + " |  _  |/ _` | '_ \\ / _` | '_ ` _ \\ / _` | '_ \\" +
                Environment.NewLine + " | | | | (_| | | | | (_| | | | | | | (_| | | | |" +
                Environment.NewLine + " \\_| |_/\\__,_|_| |_|\\__, |_| |_| |_|\\__,_|_| |_|" +
                Environment.NewLine + "                     __/ |                     " +
                Environment.NewLine + "                    |___/                      ";

            Console.Clear();

            // Создаем основное меню
            ConsoleMenu mainMenu = new ConsoleMenu("==>");
            mainMenu.Header = headerText;
            mainMenu.SubTitle = "------------------ Hangman ---------------------";

            // Добавляем пункты меню
            mainMenu.addMenuItem(0, "Create New Room", gameUi.CreateRoom);
            mainMenu.addMenuItem(2, "Join to Room", gameUi.ShowAllRooms);
            mainMenu.addMenuItem(3, "Show Leader Board", gameUi.ShowLeaderBoard);
            mainMenu.addMenuItem(4, "How to Play", ShowRules);
            mainMenu.addMenuItem(5, "Exit", Exit);

            // Отображаем меню
            mainMenu.showMenu();

        }

        // Функция выхода
        public static void Exit()
        {
            Console.WriteLine("Thanks for playing!");
            Environment.Exit(0);
        }

        // Функция отображения правил игры
        public static void ShowRules()
        {
            Console.Clear();
            Console.WriteLine("How to Play Hangman:");
            Console.WriteLine("1. Guess the word by suggesting letters.");
            Console.WriteLine("2. You have a limited number of attempts.");
            Console.WriteLine("3. Each incorrect guess reveals a part of the hangman.");
            Console.WriteLine("4. If the hangman is completed, you lose.");
            Console.WriteLine("5. Guess the word before running out of attempts!");
            Console.WriteLine("\nPress any key to return to the main menu.");
            Console.ReadKey(true);
        }
    }
}
