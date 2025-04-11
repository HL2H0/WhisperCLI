using WhisperCLI.Objects;
using System.Text.Json;

namespace WhisperCLI
{
    internal class Program
    {
        public Dictionary<string, User> Users { get; set; } = new();

        private User _currentUser;

        static void Main(string[] args)
        {
            Program program = new Program();
            program.LoadUsers();
            program.WriteHeader();

            while (true)
            {
                if (program._currentUser == null)
                {
                    program.Login();
                }
                else
                {
                    Console.Write("> ");
                    string command = Console.ReadLine();
                    switch (command.ToLower())
                    {
                        case "/logout":
                            program._currentUser = null;
                            Console.WriteLine("Logged out.\n\n");
                            break;

                        case "/changelog":
                            Console.WriteLine("V0.1");
                            Console.WriteLine("Initial release with basic functionality.\n\n");
                            break;

                        case "/help":
                            Console.WriteLine("\n\n---------------------------------------");
                            Console.WriteLine("Available commands: ");
                            Console.WriteLine("/help - Show this help message");
                            Console.WriteLine("/send - Send a message to another user");
                            Console.WriteLine("/inbox - View your inbox");
                            Console.WriteLine("/changelog - View the changelog");
                            Console.WriteLine("/logout - Logout of your account");
                            Console.WriteLine("/exit - Exit the application");
                            Console.WriteLine("---------------------------------------\n\n");
                            break;

                        case "/send":
                            Console.Write("\n\nEnter recipient username: ");
                            string recipient = Console.ReadLine();
                            if (program.Users.TryGetValue(recipient.ToLower(), out var recipientUser))
                            {
                                Console.Write("Enter message content: ");
                                string content = Console.ReadLine();
                                program.SendMessage(recipientUser, content);
                                program.WriteHeader();
                                Console.WriteLine("Message sent!\n\n");
                            }
                            else
                            {
                                Console.WriteLine($"User {recipient} not found.\n\nw");
                            }
                            break;

                        case "/inbox":
                            program.ViewInbox();
                            break;

                        case "/exit":
                            Console.WriteLine("Goodbye!");
                            return;

                        default:
                            Console.WriteLine("Unknown Command. Type /help for a list of commands.\n");
                            break;
                    }
                }
            }
        }


        public void LoadUsers()
        {
            if (File.Exists("Data/UserDatabase.json"))
            {
                var json = File.ReadAllText("Data/UserDatabase.json");
                if (string.IsNullOrEmpty(json))
                {
                    Users = new Dictionary<string, User>();
                }
                else
                {
                    JsonSerializerOptions jsonOptions = new() { PropertyNameCaseInsensitive = true };
                    Users = JsonSerializer.Deserialize<Dictionary<string, User>>(json);
                }
            }
        }
        public void SaveUsers()
        {
            JsonSerializerOptions jsonOptions = new() { WriteIndented = true };
            var json = JsonSerializer.Serialize(Users, jsonOptions);
            File.WriteAllText("Data/UserDatabase.json", json);
        }

        public void Login()
        {
            Console.Write("Please enter your username: ");
            string username = Console.ReadLine();
            if (Users.TryGetValue(username.ToLower(), out var user))
            {
                Console.Write("Please enter your password: ");
                string password = Console.ReadLine();
                if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
                {
                    Console.WriteLine("Incorrect password. Please try again.\n\n");
                    return;
                }
                _currentUser = user;
                WriteHeader();
                Console.WriteLine($"Welcome back, {user.Username}!\n\n");
            }
            else
            {
                Console.Write($"User {username} not found. Would you like to create a new user? (y/n)");
                string response = Console.ReadLine();
                if (response.ToLower() == "y")
                {
                    Console.Write("\n\nCreate a new password: ");
                    string password = Console.ReadLine();
                    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
                    _currentUser = new User { Username = username.ToLower(), Password = hashedPassword };
                    Users[username.ToLower()] = _currentUser;
                    SaveUsers();
                    WriteHeader();
                    Console.WriteLine($"User {username} created!\n\n");
                }
                else
                {
                    return;
                }
            }
        }

        public void SendMessage(User recipient, string content)
        {
            var message = new Message
            {
                From = _currentUser,
                Content = content,
                TimeStamp = DateTime.Now
            };
            recipient.Inbox.Add(message);
            SaveUsers();
        }

        public void ViewInbox()
        {
            if (_currentUser.Inbox.Count == 0)
            {
                WriteHeader();
                Console.WriteLine("Your inbox is empty. )=\n\n");
                return;
            }
                            
            WriteHeader();
            Console.WriteLine("Your inbox:");
            foreach (var message in _currentUser.Inbox)
            {
                Console.WriteLine("\n\n---------------------------------------");
                Console.WriteLine($"From: {message.From.Username},\nTime: {message.TimeStamp}, \nContent: {message.Content}");
                Console.WriteLine("---------------------------------------\n\n");
            }
        }

        public void WriteHeader()
        {
            Console.Clear();
            Console.WriteLine(" _       ____    _                         ________    ____");
            Console.WriteLine("| |     / / /_  (_)________  ___  _____   / ____/ /   /  _/");
            Console.WriteLine("| | /| / / __ \\/ / ___/ __ \\/ _ \\/ ___/  / /   / /    / /  ");
            Console.WriteLine("| |/ |/ / / / / (__  ) /_/ /  __/ /     / /___/ /____/ /   ");
            Console.WriteLine("|__/|__/_/ /_/_/____/ .___/\\___/_/      \\____/_____/___/   ");
            Console.WriteLine("                     /_/                                   \n\n\n\n");
        }
    }
}
