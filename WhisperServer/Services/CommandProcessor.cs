using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhisperServer.Models;
using System.Text.Json;

namespace WhisperServer.Services
{
    class CommandProcessor
    {
        private Dictionary<string, User> _users;

        public CommandProcessor(Dictionary<string, User> users)
        {
            _users = users;
        }

        public string Process(string json)
        {
            var cmd = JsonSerializer.Deserialize<Command>(json);
            switch (cmd.Type.ToLower())
            {
                case "login":
                    return HandleLogin(cmd);
                case "register":
                    return HandleRegister(cmd);
                case "send_message":
                    return HandleSend(cmd);
                case "get_inbox":
                    return HandleInbox(cmd);
                default:
                    return JsonSerializer.Serialize(new { status = "error", message = "Unknown command" });
            }
        }

        private string HandleLogin(Command cmd)
        {
            if (_users.TryGetValue(cmd.Username.ToLower(), out var user) && BCrypt.Net.BCrypt.Verify(cmd.Password, user.Password))
            {
                return JsonSerializer.Serialize(new { status = "success", message = "Logged in" });
            }
            else
            {
                return JsonSerializer.Serialize(new { status = "fail", message = "Invalid username or password" });
            }
        }

        private string HandleRegister(Command cmd)
        {
            var uname = cmd.Username.ToLower();
            if (_users.ContainsKey(uname))
            {
                return JsonSerializer.Serialize(new { status = "fail", message = "Username already exists" });
            }
            _users[uname] = new User
            {
                Username = uname,
                Password = BCrypt.Net.BCrypt.HashPassword(cmd.Password),
            };
            return JsonSerializer.Serialize(new { status = "success", message = "User registered" });
        }

        private string HandleSend(Command cmd)
        {
            if(!_users.TryGetValue(cmd.To.ToLower(), out var recipient))
                return JsonSerializer.Serialize(new { status = "fail", message = "Recipient not found" });

            recipient.Inbox.Add(new Message
            {
                From = cmd.From,
                Content = cmd.Content,
                Timestamp = DateTime.UtcNow
            });

            return JsonSerializer.Serialize(new { message = "sent" });
        }

        private string HandleInbox(Command cmd)
        {
            if(!_users.TryGetValue(cmd.From.ToLower(), out var user))
                return JsonSerializer.Serialize(new { status = "fail", message = "User not found" });

            return JsonSerializer.Serialize(user.Inbox);
        }
    }
}
