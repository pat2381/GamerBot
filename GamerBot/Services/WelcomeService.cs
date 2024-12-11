using Discord.WebSocket;
using GamerBot.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamerBot.Services
{
    public class WelcomeService
    {
        private readonly DiscordSocketClient _client;
        private readonly Config _config;
        private readonly ILogger<WelcomeService> _logger;

        public WelcomeService(DiscordSocketClient client, Config config, ILogger<WelcomeService> logger)
        {
            _client = client;
            _config = config;
            _logger = logger;
        }

        public void Initialize()
        {
            _client.UserJoined += OnUserJoinedAsync;
        }

        private async Task OnUserJoinedAsync(SocketGuildUser user)
        {
            // Falls kein WelcomeChannelId gesetzt ist oder 0, nichts tun
            if (_config.WelcomeChannelId == 0)
            {
                _logger.LogWarning("Willkommenschannel ist nicht konfiguriert (WelcomeChannelId = 0).");
                return;
            }

            var channel = user.Guild.GetTextChannel(_config.WelcomeChannelId);
            if (channel == null)
            {
                _logger.LogWarning($"Willkommenschannel mit ID {_config.WelcomeChannelId} wurde nicht gefunden.");
                return;
            }

            // Nachricht formatieren
            // {Mention} wird durch user.Mention ersetzt
            var welcomeMessage = _config.WelcomeMessage.Replace("{Mention}", user.Mention);

            await channel.SendMessageAsync(welcomeMessage);
        }
    }
}
