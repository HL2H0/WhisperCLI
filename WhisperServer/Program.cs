using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using WhisperServer.Models;
using WhisperServer.Services;

namespace WhisperServer
{
    internal class Program
    {
        static Dictionary<string, User> Users = new();
        static CommandProcessor Processor;

        static void Main(string[] args)
        {
            LoadUsers(  );
            Processor = new CommandProcessor(Users);

            TcpListener server = new TcpListener(System.Net.IPAddress.Any, 8080);
            server.Start();
            Console.WriteLine("[SERVER] Whisper Server is running on port 8080...");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();

                IPEndPoint remoteIpEndPoint = client.Client.RemoteEndPoint as IPEndPoint;

                Console.WriteLine($"[CLIENT] Client connected from : {remoteIpEndPoint}");
                Thread clientThread = new (() => HandleClient(client));
                clientThread.Start();
            }
        }

        static void HandleClient(TcpClient client)
        {
            using NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[4069];
            int byteCount;

            try
            {
                while((byteCount = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    string received = Encoding.UTF8.GetString(buffer, 0, byteCount);
                    Console.WriteLine($"[RECV] {received}");

                    string response = Processor.Process(received);
                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseBytes, 0, responseBytes.Length);
                    Console.WriteLine($"[SEND] {response}");
                    SaveUsers();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Client error : {ex.Message}");
            }
            client.Close();
            Console.WriteLine("[CLIENT] Client disconnected.");
        }

        static void LoadUsers()
        {
            if (!File.Exists("UserDatabase.json")) return;
            string json = File.ReadAllText("UserDatabase.json");
            Users = JsonSerializer.Deserialize<Dictionary<string, User>>(json) ?? new Dictionary<string, User>();
        }

        static void SaveUsers()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText("UserDatabase.json", JsonSerializer.Serialize(Users, options));
        }
    }
}
