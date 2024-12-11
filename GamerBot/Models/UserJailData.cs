using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamerBot.Models
{
    public class UserJailData
    {
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public ulong GuildId { get; set; }
        public DateTimeOffset JailEndTime { get; set; } // Zeitpunkt der automatischen Entsperrung
        public string OriginalRolesJson { get; set; } // JSON-liste der ursprünglichen Rollen
        public bool IsJailed { get; set; } // Ob der User gerade eingesperrt ist
    }
}
