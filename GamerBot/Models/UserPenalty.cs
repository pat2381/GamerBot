using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamerBot.Models
{
    public class UserPenalty
    {
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public ulong GuildId { get; set; }
        public int TotalPoints { get; set; } // Gesamte Strafpunkte des Nutzers
        public int OffenseCount { get; set; } // Anzahl vergangener Verstöße
    }
}
