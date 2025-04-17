using System.Text.Json;
using WhisperCLI.Models;
namespace WhisperCLI
{
    internal class Program
    {

        static void Main(string[] args)
        {
            Client client = new Client();
            User _currentUser = null;
            if (args.Count() > 0)
            {
                if (args[0] == "local")
                {
                    client.Connect("", 8080);
                }
            }
            else
            {
                client.Connect("", 8080);
            }
            
            
            

            WriteHeader();

            while(true)
            {
                Console.WriteLine("Hi, What would you like to do?");
                Console.WriteLine("Login (1)");
                Console.WriteLine("Register (2)");
                Console.WriteLine("Exit (3)");
                switch (Console.ReadLine())
                {
                    case "1":
                        Console.Write("Enter your username:");
                        string username = Console.ReadLine();
                        Console.Write("Enter your password:");
                        string password = Console.ReadLine();
                        client.Send(JsonSerializer.Serialize(new Command{ Type = "login", Username = username, Password = password }));
                        Thread.Sleep(1000); // Wait for server response
                        if (client.ReceivedMessage != string.Empty)
                        {
                            var response = JsonSerializer.Deserialize<object>(client.ReceivedMessage);
                            var responseObj = (JsonElement)response;
                            var responseType = responseObj.GetProperty("status").GetString();
                            var responseMessage = responseObj.GetProperty("message").GetString();

                            if (responseType == "success")
                            {
                                Console.WriteLine("Login successful!");
                                _currentUser = new User { Username = username, Password = password };
                                break;
                            }
                            else
                            {
                                Console.WriteLine(responseMessage);
                            }
                        }
                        else
                        {
                            Console.WriteLine("No response from server.");
                        }
                        client.ResetReceivedMessage();
                        break;
                    case "2":
                        Console.Write("Create Username: ");
                        string newUsername = Console.ReadLine();
                        Console.Write("Create Password: ");
                        string newPassword = Console.ReadLine();
                        client.Send(JsonSerializer.Serialize(new Command{ Type = "register", Username = newUsername, Password = newPassword }));
                        Thread.Sleep(1000); // Wait for server response
                        if (client.ReceivedMessage != string.Empty)
                        {
                            var response = JsonSerializer.Deserialize<object>(client.ReceivedMessage);
                            var responseObj = (JsonElement)response;
                            var responseType = responseObj.GetProperty("status").GetString();
                            var responseMessage = responseObj.GetProperty("message").GetString();

                            if (responseType == "success")
                            {
                                Console.WriteLine("Registration successful!");
                                _currentUser = new User { Username = newUsername, Password = newPassword };
                                break;
                            }
                            else if (responseType == "fail")
                            {
                                Console.WriteLine("Username already exists. Please try again.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("No response from server.");
                        }
                        client.ResetReceivedMessage();
                        break;

                    case "3":
                        Console.WriteLine("Exiting...");
                        Thread.Sleep(1000);
                        client.Disconnect();
                        Environment.Exit(0);
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }

                        }
                        else
                        {
                            Console.WriteLine("No response from server.");
                        }
                        client.ResetReceivedMessage();
                        break;
                }
                if(command.StartsWith("send "))
                {
                    var parts = command.Split(' ', 3);
                    if (parts.Length < 3)
                    {
                        Console.WriteLine("Usage: send <username> <message>");
                        continue;
                    }
                    string recipient = parts[1];
                    string message = parts[2];
                    client.Send(JsonSerializer.Serialize(new Command { Type = "send_message", From = _currentUser.Username, To = recipient, Content = message }));
                    Thread.Sleep(1000); // Wait for server response
                    if (client.ReceivedMessage != string.Empty)
                    {
                        var response = JsonSerializer.Deserialize<object>(client.ReceivedMessage);
                        var responseObj = (JsonElement)response;
                        var responseType = responseObj.GetProperty("status").GetString();
                        var responseMessage = responseObj.GetProperty("message").GetString();
                        if (responseType == "success")
                        {
                            Console.WriteLine(responseMessage);
                        }
                        else
                        {
                            Console.WriteLine(responseMessage);
                        }
                    }
                    else
                    {
                        Console.WriteLine("No response from server.");
                    }
                }
            }
        }

        static void WriteHeader()
        {
            Console.Clear();
            Console.WriteLine(" _       ____    _                         ________    ____");
            Console.WriteLine("| |     / / /_  (_)________  ___  _____   / ____/ /   /  _/");
            Console.WriteLine("| | /| / / __ \\/ / ___/ __ \\/ _ \\/ ___/  / /   / /    / /  ");
            Console.WriteLine("| |/ |/ / / / / (__  ) /_/ /  __/ /     / /___/ /____/ /   ");
            Console.WriteLine("|__/|__/_/ /_/_/____/ .___/\\___/_/      \\____/_____/___/   ");
            Console.WriteLine("                     /_/                                   \n\n");
        }
    }
}
