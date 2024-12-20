using System;
using System.Net.Sockets;

class Program
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

                // Меню выбора
                Menu.ShowMenu(stream);
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
}
