using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamerBot.Models
{
    public class Config
    {
        public string BotToken { get; set; }
        public string Prefix { get; set; }

        // Verbote Wörter-Einstellungen
        public string ForbiddenWordsFilePath { get; set; }

        // Strafpunkte & Multiplikatoren
        public int MaxPenaltyPoints { get; set; }
        public Dictionary<int, double> PenaltyMultipliers { get; set; }

        // Zensur
        public string CensorString { get; set; }

        // Moderation & Jail-Einstellungen
        public ulong ModerationChannelId { get; set; }
        public string JailRoleName { get; set; }
        public ulong JailChannelId { get; set; }
        public int DefaultJailTimeMinutes { get; set; }

        // Sonstige (aus vorherigen Beispielen)
        public ulong WelcomeChannelId { get; set; }
        public string WelcomeMessage { get; set; }
        public int XPPerMessage { get; set; }
        public string LevelCurve { get; set; }
        public int LeaderboardLimit { get; set; }
        //public string[] ForbiddenWords { get; set; } // Falls noch benötigt, ansonsten entfernen
        public ulong ModerationAlertChannelId { get; set; } // Falls noch benötigt
        public Dictionary<int, string> LevelRoles { get; set; }
        public string DatabaseFile { get; set; }
    }
}
