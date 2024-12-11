using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamerBot.Models
{
    public class PenaltyResult
    {
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public int NewTotalPoints { get; set; }
        public int OffenseCount { get; set; }
        public int AddedPoints { get; set; }
        public bool ReachedMaxPoints { get; set; }
    }
}
