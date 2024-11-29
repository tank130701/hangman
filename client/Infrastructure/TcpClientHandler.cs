using System;
using System.IO;
using System.Net.Sockets;

namespace HangmanClient.Infrastructure
{
    public class TcpClientHandler
    {
        private readonly TcpClient _client;
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;

        public TcpClientHandler(string address, int port)
        {
            _client = new TcpClient(address, port);
            _reader = new StreamReader(_client.GetStream());
            _writer = new StreamWriter(_client.GetStream()) { AutoFlush = true };
        }

        public void SendMessage(string message)
        {
            _writer.WriteLine(message);
        }

        public string ReceiveMessage()
        {
            return _reader.ReadLine();
        }

        public void Close()
        {
            _client.Close();
        }
    }
}
