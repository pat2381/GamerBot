using Discord.Interactions;
using Discord;
using Microsoft.Extensions.Logging;

using GamerBot.Services;
using Discord.WebSocket;

namespace GamerBot.Modules
{
    public class DurationModalModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ILogger<DurationModalModule> _logger;
        // Hier können wir den JailService oder andere Services injecten, wenn vorhanden
        private readonly JailService _jailService;

        public DurationModalModule(ILogger<DurationModalModule> logger, JailService jailService)
        {
            _logger = logger;
            _jailService = jailService;
        }

        [ModalInteraction("durationmodal:*:*")]
        public async Task HandleDurationModalAsync(string action, string userIdStr)
        {
            if (!ulong.TryParse(userIdStr, out ulong targetUserId))
            {
                await RespondAsync("Ungültige User-ID.", ephemeral: true);
                return;
            }

            var guildUser = Context.Guild.GetUser(targetUserId);
            if (guildUser == null)
            {
                await RespondAsync("Dieser User ist nicht mehr auf dem Server.", ephemeral: true);
                return;
            }

            var modalInteraction = (SocketModal)Context.Interaction;

            // Suche nach dem Text-Feld mit der CustomId "duration_input"
            var durationComponent = modalInteraction.Data.Components.FirstOrDefault(c => c.CustomId == "duration_input");
            if (durationComponent == null)
            {
                await RespondAsync("Fehler: Das Dauer-Feld wurde nicht gefunden.", ephemeral: true);
                return;
            }

            var durationStr = durationComponent.Value;
            if (!int.TryParse(durationStr, out int minutes))
            {
                await RespondAsync("Bitte eine gültige Zahl eingeben.", ephemeral: true);
                return;
            }

            var duration = TimeSpan.FromMinutes(minutes);
            if (duration.TotalMinutes <= 0)
            {
                await RespondAsync("Die Dauer muss größer als 0 sein.", ephemeral: true);
                return;
            }

            switch (action)
            {
                case "timeout":
                    await HandleTimeoutAsync(guildUser, duration);
                    break;
                case "jail":
                    await HandleJailAsync(guildUser, duration);
                    break;
                default:
                    await RespondAsync("Unbekannte Aktion.", ephemeral: true);
                    break;
            }
        }

        private async Task HandleTimeoutAsync(IGuildUser user, TimeSpan duration)
        {
            await user.SetTimeOutAsync(duration);
            await RespondAsync($"{user.Mention} wurde für {duration.TotalMinutes} Minuten stummgeschaltet.", ephemeral: false);
            await InformUserDM(user, $"Du wurdest für {duration.TotalMinutes} Minuten stummgeschaltet.");
        }

        private async Task HandleJailAsync(IGuildUser user, TimeSpan duration)
        {
            // Hier würde später der JailService aufgerufen, um den User einzusperren.
            // Vorläufig nur eine Dummy-Nachricht:
            await _jailService.JailUserAsync(user, duration);
            await RespondAsync($"{user.Mention} wurde für {duration.TotalMinutes} Minuten in den Jail gesteckt.", ephemeral: false);
            await InformUserDM(user, $"Du wurdest für {duration.TotalMinutes} Minuten in den Jail gesteckt.");
            // JailService.Logic(user, duration);
        }

        private async Task InformUserDM(IGuildUser user, string message)
        {
            try
            {
                var dm = await user.CreateDMChannelAsync();
                await dm.SendMessageAsync(message);
            }
            catch { }
        }
    }
}
