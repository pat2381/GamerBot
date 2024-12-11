using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System.Reflection;


namespace GamerBot.Services
{
    public class InteractionHandlingService
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactions;
        private readonly IServiceProvider _services;
        private readonly ILogger<InteractionHandlingService> _logger;

        public InteractionHandlingService(DiscordSocketClient client, InteractionService interactions, IServiceProvider services, ILogger<InteractionHandlingService> logger)
        {
            _client = client;
            _interactions = interactions;
            _services = services;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            // Module mit Slash-Commands laden
            await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            _client.InteractionCreated += HandleInteractionAsync;
            _client.Ready += ClientReadyAsync;
            // Optionale: Globale Commands oder Guild-spezifische Commands?
            // Für Entwicklung: Slash-Commands auf eine bestimmte Test-Guild registrieren, um schneller zu updaten.
            // Hier ein Beispiel für globale Commands:
            //await _interactions.RegisterCommandsGloballyAsync(true);

            
        }

        private async Task ClientReadyAsync()
        {
            // Jetzt ist der Client bereit, jetzt können wir die Commands registrieren.
            await _interactions.RegisterCommandsGloballyAsync(true);

            //Wenn du die SlashCommands schnell verwenden willst dann füge diese direkt der Gilde hinzu, so sollten die Commands in wenigen Sek, verfügbar sein.
            //await _interactions.RegisterCommandsToGuildAsync(guildId, true);

        }

        private async Task HandleInteractionAsync(SocketInteraction interaction)
        {
            try
            {
                var ctx = new SocketInteractionContext(_client, interaction);
                var result = await _interactions.ExecuteCommandAsync(ctx, _services);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning($"Interaction Error: {result.ErrorReason}");
                    if (interaction.HasResponded == false)
                    {
                        await interaction.RespondAsync($"Ein Fehler ist aufgetreten: {result.ErrorReason}", ephemeral: true);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Verarbeiten einer Interaktion.");
                if (!interaction.HasResponded)
                {
                    await interaction.RespondAsync("Ein unerwarteter Fehler ist aufgetreten.", ephemeral: true);
                }
            }
        }
    }
}
