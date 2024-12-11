using Discord.Commands;
using Discord.WebSocket;
using Discord;
using GamerBot.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GamerBot.Data.Repository;
using Discord.Interactions;

namespace GamerBot.Services
{
    public class BotStartupService : IHostedService
    {
        private readonly IServiceProvider _services;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly ILogger<BotStartupService> _logger;
        private readonly Config _config;

        public BotStartupService(
            IServiceProvider services,
            DiscordSocketClient client,
            CommandService commands,
            Config config,
            ILogger<BotStartupService> logger)
        {
            _services = services;
            _client = client;
            _commands = commands;
            _config = config;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _client.Log += LogAsync;
            _client.Ready += ClientReadyAsync;

            // Command Handling Service aufsetzen
            var commandHandling = ActivatorUtilities.CreateInstance<CommandHandlingService>(_services);
            await commandHandling.InitializeAsync();

            // Moderation zuerst
            var moderationService = _services.GetRequiredService<ModerationService>();
            moderationService.Initialize();

            // XP Service initialisieren
            var xpService = _services.GetRequiredService<XPService>();
            xpService.Initialize();

            //Willkommens Service initialisieren
            var welcomeService = _services.GetRequiredService<WelcomeService>();
            welcomeService.Initialize();

            // Discord Login & Start
            await _client.LoginAsync(TokenType.Bot, _config.BotToken);
            await _client.StartAsync();

 

            // Interaction-Handhabung nach dem Start initialisieren
            var interactionHandling = _services.GetRequiredService<InteractionHandlingService>();
            await interactionHandling.InitializeAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _client.StopAsync();
            await _client.LogoutAsync();
        }

        private Task LogAsync(LogMessage msg)
        {
            _logger.LogInformation(msg.ToString());
            return Task.CompletedTask;
        }

        private async Task ClientReadyAsync()
        {
            _logger.LogInformation($"Bot ist angemeldet als {_client.CurrentUser.Username}#{_client.CurrentUser.Discriminator}");

            // Hier rufen wir nun den StartupDataLoadService auf:
            var startupDataLoad = _services.GetRequiredService<StartupDataLoadService>();
            await startupDataLoad.LoadForbiddenWordsAsync();
        }
    }
}
