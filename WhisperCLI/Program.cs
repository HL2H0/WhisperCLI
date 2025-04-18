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

            if (args.Length > 0 && args[0] == "local")
            {
                client.Connect("127.0.0.1", 8080);
            }
            else
            {
                client.Connect("127.0.0.1", 8080);
            }

            WriteHeader();

            while (true)
            {
                Console.WriteLine("Hi, What would you like to do?");
                Console.WriteLine("Login (1)");
                Console.WriteLine("Register (2)");
                Console.WriteLine("Exit (3)");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.Write("Enter your username: ");
                        string username = Console.ReadLine();
                        Console.Write("Enter your password: ");
                        string password = Console.ReadLine();
                        client.Send(JsonSerializer.Serialize(new Command { Type = "login", Username = username, Password = password }));
                        Thread.Sleep(1000); // Wait for server response

                        if (!string.IsNullOrEmpty(client.ReceivedMessage))
                        {
                            var response = JsonSerializer.Deserialize<JsonElement>(client.ReceivedMessage);
                            string responseType = response.GetProperty("status").GetString();
                            string responseMessage = response.GetProperty("message").GetString();

                            if (responseType == "success")
                            {
                                Console.WriteLine("Login successful!");
                                _currentUser = new User { Username = username, Password = password };
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
                        client.Send(JsonSerializer.Serialize(new Command { Type = "register", Username = newUsername, Password = newPassword }));
                        Thread.Sleep(1000); // Wait for server response

                        if (!string.IsNullOrEmpty(client.ReceivedMessage))
                        {
                            var response = JsonSerializer.Deserialize<JsonElement>(client.ReceivedMessage);
                            string responseType = response.GetProperty("status").GetString();
                            string responseMessage = response.GetProperty("message").GetString();

                            if (responseType == "success")
                            {
                                Console.WriteLine("Registration successful!");
                                _currentUser = new User { Username = newUsername, Password = newPassword };
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

                if (_currentUser != null)
                {
                    break;
                }
            }

            WriteHeader();
            Console.WriteLine($"Welcome, {_currentUser.Username}!");

            while (true)
            {
                Console.Write("> ");
                string command = Console.ReadLine();

                if (command == "help")
                {
                    Console.WriteLine("Available commands:");
                    Console.WriteLine("send <username> <message> - Send a message to a user.");
                    Console.WriteLine("inbox - View your inbox.");
                    Console.WriteLine("logout - Logout from the current session.");
                }
                else if (command == "logout")
                {
                    Console.WriteLine("Logging out...");
                    client.Disconnect();
                    _currentUser = null;
                    WriteHeader();
                    break;
                }
                else if (command == "inbox")
                {
                    Console.WriteLine("Your inbox:");
                    client.Send(JsonSerializer.Serialize(new Command { Type = "get_inbox", From = _currentUser.Username }));
                    Thread.Sleep(1000); // Wait for server response

                    if (!string.IsNullOrEmpty(client.ReceivedMessage))
                    {
                        var response = JsonSerializer.Deserialize<List<Message>>(client.ReceivedMessage);
                        if (response != null && response.Count > 0)
                        {
                            var todayMessages = response.Where(m => m.Timestamp.Date == DateTime.Now.Date);
                            var yesterdayMessages = response.Where(m => m.Timestamp.Date == DateTime.Now.AddDays(-1).Date);
                            var olderMessages = response.Where(m => m.Timestamp.Date < DateTime.Now.AddDays(-1).Date);

                            if (todayMessages.Any())
                            {
                                Console.WriteLine("=== Today ===");
                                foreach (var message in todayMessages)
                                {
                                    Console.WriteLine($"[{message.Timestamp.ToShortTimeString()}] {message.From} | {message.Content}");
                                }
                            }

                            if (yesterdayMessages.Any())
                            {
                                Console.WriteLine("=== Yesterday ===");
                                foreach (var message in yesterdayMessages)
                                {
                                    Console.WriteLine($"[{message.Timestamp.ToShortTimeString()}] {message.From} | {message.Content}");
                                }
                            }

                            if (olderMessages.Any())
                            {
                                Console.WriteLine("=== Older Messages ===");
                                foreach (var message in olderMessages)
                                {
                                    Console.WriteLine($"[{message.Timestamp.ToShortTimeString()}] {message.From} | {message.Content}");
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("No messages in your inbox.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No response from server.");
                    }
                    client.ResetReceivedMessage();
                }
                else if (command.StartsWith("send "))
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

                    if (!string.IsNullOrEmpty(client.ReceivedMessage))
                    {
                        var response = JsonSerializer.Deserialize<JsonElement>(client.ReceivedMessage);
                        string responseType = response.GetProperty("status").GetString();
                        string responseMessage = response.GetProperty("message").GetString();

                        Console.WriteLine(responseMessage);
                    }
                    else
                    {
                        Console.WriteLine("No response from server.");
                    }
                }
                else
                {
                    Console.WriteLine("Unknown command. Please try again.");
                    Console.WriteLine("Use 'help' to see available commands.");
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
