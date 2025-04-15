using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WhisperCLI
{
    public class Client
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private Thread _receiveThread;

        public string ReceivedMessage { get; private set; } = string.Empty;

        public bool IsConnected => _client?.Connected ?? false;

        public void Connect(string serverIP, int port)
        {
            try
            {
                _client = new TcpClient();
                _client.Connect(serverIP, port);
                _stream = _client.GetStream();

                Console.WriteLine("Connected to server.");


                _receiveThread = new Thread(ReceiveLoop);
                _receiveThread.Start();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"[ERROR] Couldn't connect : {ex.Message}");
            }
        }

        public void Send(string message)
        {
            if (!IsConnected) return;

            byte[] data = Encoding.UTF8.GetBytes(message);
            _stream.Write(data, 0, data.Length);
        }

        private void ReceiveLoop()
        {
            byte[] buffer = new byte[1024];
            while(IsConnected)
            {
                try
                {
                    int bytesCount = _stream.Read(buffer, 0, buffer.Length);
                    if (bytesCount <= 0) continue;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesCount);
                    ReceivedMessage = message;
                    Console.WriteLine($"[SERVER] {message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Disconnected from server.");
                    break;
                }
            }
        }

        public void ResetReceivedMessage()
        {
            ReceivedMessage = string.Empty;
        }

        public void Disconnect()
        {
            _stream?.Close();
            _client?.Close();
        }
    }
}
