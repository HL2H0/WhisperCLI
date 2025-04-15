using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhisperCLI.Objects
{
    internal class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public List<Message> Inbox { get; set; } = new List<Message>();
    }
}
