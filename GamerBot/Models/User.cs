using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamerBot.Models
{
    public class User
    {
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public ulong GuildId { get; set; }
        public int XP { get; set; }
        public int Level { get; set; }
    }
}
