using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhisperCLI.Models
{
    class Message
    {
        public string From { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
