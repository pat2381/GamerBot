using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamerBot.Models
{
    public class Config
    {
        public string? BotToken { get; set; }
        public string? Prefix { get; set; }
        public ulong WelcomeChannelId { get; set; }
        public string? WelcomeMessage { get; set; }
        public int XPPerMessage { get; set; }
        public string? LevelCurve { get; set; }
        public int LeaderboardLimit { get; set; }
        public string[]? ForbiddenWords { get; set; }
        public ulong ModerationAlertChannelId { get; set; }
        public Dictionary<int, string>? LevelRoles { get; set; }
        public string? DatabaseFile { get; set; }
    }
}
