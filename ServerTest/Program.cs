using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;

namespace ServerTest
{
    internal class Program
    {
        private static TcpListener listener;

        static void Main(string[] args)
        {
            

            TcpListener listener = new TcpListener(IPAddress.Any, 8080);
            listener.Start();
            Console.WriteLine("Server started. Waiting for a connection...");

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("Client connected.");
                Task.Run(() => HandleClient(client)); // Handle client in a separate thread
            }
            



        }

        public static void HandleClient(TcpClient client)
        {
            NetworkStream stream;
            byte[] buffer;
            while (true)
            {
                stream = client.GetStream();
                buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                    break; // Client disconnected
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Received message: " + message);
                // Echo the message back to the client
                byte[] response = Encoding.UTF8.GetBytes("Echo: " + message);
                stream.Write(response, 0, response.Length);
            }

            stream.Close();
            client.Close();
            listener.Stop();
        }
    }
}
