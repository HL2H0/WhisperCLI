using System.IO;
using System.Net.Sockets;
using System.Text;


namespace ClientTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TcpClient tcpClient;
            NetworkStream stream;
            while (true)
            {
                try
                {
                    tcpClient = new TcpClient("127.0.0.1", 8080);
                }
                catch (SocketException ex)
                {
                    Console.WriteLine("Couldn't Connect to server\nTrying again in a moment\n\n");
                    tcpClient = null;
                }
                if (tcpClient != null)
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Retrying connection...");
                    System.Threading.Thread.Sleep(1000); // Wait for 1 second before retrying
                }
            }


            Console.WriteLine("Connected to server.");
            Console.WriteLine("Sending message to server...");
            while (true)
            {
                stream = tcpClient.GetStream();
                string message = Console.ReadLine();
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
                if (message == "exit")
                {
                    break; // Exit the loop if the user types "exit"
                }
            }
            //stream = tcpClient.GetStream();

            //string message = "Hello, Server!";
            //byte[] data = Encoding.UTF8.GetBytes(message);
            //stream.Write(data, 0, data.Length);

            stream.Close();
            tcpClient.Close();
            //Console.WriteLine("Message sent to server: " + message);
            Console.ReadKey();
        }
    }
}
