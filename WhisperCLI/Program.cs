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
            Console.WriteLine("Welcome to Whisper CLI!");
            while (true)
            {
                Console.WriteLine("Please enter your username:");
                string username = Console.ReadLine();
                if (program.Users.TryGetValue(username.ToLower(), out var user))
                {
                    program._currentUser = user;
                    Console.WriteLine($"Welcome back, {user.Username}!");
                    break;
                }
                else
                {
                    Console.WriteLine($"User {username} not found. Would you like to create a new user? (y/n)");
                    string response = Console.ReadLine();
                    if (response.ToLower() == "y")
                    {
                        program._currentUser = new User { Username = username.ToLower() };
                        program.Users[username.ToLower()] = program._currentUser;
                        program.SaveUsers();
                        Console.WriteLine($"User {username} created!");
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Goodbye!");
                        return;
                    }
                }
            }
            while (true)
            {
                Console.Write("Enter Command : ");
                string command = Console.ReadLine();
                switch(command.ToLower())
                {
                    case "send":
                        Console.WriteLine("Enter recipient username:");
                        string recipient = Console.ReadLine();
                        if (program.Users.TryGetValue(recipient.ToLower(), out var recipientUser))
                        {
                            Console.WriteLine("Enter message content:");
                            string content = Console.ReadLine();
                            program.SendMessage(recipientUser, content);
                            Console.WriteLine("Message sent!");
                        }
                        else
                        {
                            Console.WriteLine($"User {recipient} not found.");
                        }
                        break;
                    case "inbox":
                        program.ViewInbox();
                        break;
                    case "exit":
                        return;
                    default:
                        Console.WriteLine("Unknown command.");
                        break;
                }
            }
        }

        public void LoadUsers()
        {
            if (File.Exists("Data/UserDatabase.json"))
            {
                var json = File.ReadAllText("Data/UserDatabase.json");
                Users = JsonSerializer.Deserialize<Dictionary<string, User>>(json);
            }
        }
        public void SaveUsers()
        {
            var json = JsonSerializer.Serialize(Users);
            File.WriteAllText("Data/UserDatabase.json", json);
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
                Console.WriteLine("Your inbox is empty. )=");
                return;
            }

            Console.WriteLine("Your inbox:");
            foreach (var message in _currentUser.Inbox)
            {
                Console.WriteLine($"From: {message.From.Username},\nTime: {message.TimeStamp}, \nContent: {message.Content}");
            }
        }
    }
}
