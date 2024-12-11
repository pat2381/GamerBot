using Discord.Interactions;
using Discord;
using GamerBot.Models;
using Microsoft.Extensions.Logging;
using GamerBot.Services;


namespace GamerBot.Modules
{
    public class ModerationActionsModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ILogger<ModerationActionsModule> _logger;
        private readonly Config _config;
        // Hier benötigen wir später den JailService, etc., wenn wir ihn implementiert haben.
        private readonly JailService _jailService;

        public ModerationActionsModule(Config config, ILogger<ModerationActionsModule> logger, JailService jailService)
        {
            _config = config;
            _logger = logger;
            _jailService = jailService;
        }

        [ComponentInteraction("action:*:*")]
        public async Task HandleActionButtonAsync(string action, string userIdStr)
        {
            // Unverändert für den Kick-Button:
            // Da Kick kein Modal braucht, können wir es hier lassen.
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

            if (!((IGuildUser)Context.User).GuildPermissions.KickMembers)
            {
                await RespondAsync("Du hast nicht die Berechtigung, diese Aktion durchzuführen.", ephemeral: true);
                return;
            }

            // Nur der Kick-Case bleibt hier übrig
            if (action == "kick")
            {
                await HandleKickAsync(guildUser);
            }
            else
            {
                await RespondAsync("Unbekannte Aktion oder Aktion erfordert Modal.", ephemeral: true);
            }
        }

        [ComponentInteraction("setduration:*:*")]
        public async Task HandleSetDurationButtonAsync(string action, string userIdStr)
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

            if (!((IGuildUser)Context.User).GuildPermissions.KickMembers)
            {
                await RespondAsync("Du hast nicht die Berechtigung, diese Aktion durchzuführen.", ephemeral: true);
                return;
            }

            // Hier zeigen wir jetzt ein Modal an:
            var modalId = $"durationmodal:{action}:{targetUserId}";
            var mb = new ModalBuilder()
                .WithTitle("Dauer einstellen")
                .WithCustomId(modalId)
                .AddTextInput("Dauer in Minuten", "duration_input", placeholder: "z. B. 30", required: true);

            await RespondWithModalAsync(mb.Build());
        }

        private async Task HandleTimeoutAsync(IGuildUser user)
        {
            // Timeout Feature: Discord unterstützt Timeouts (Benutzer stumm schalten).
            // Hier nehmen wir einen festen Wert aus der Config oder fragen den Mod in Zukunft interaktiv.
            var duration = TimeSpan.FromMinutes(30); // Beispiel: 30 Minuten
            await user.SetTimeOutAsync(duration);

            await RespondAsync($"{user.Mention} wurde für {duration.TotalMinutes} Minuten stummgeschaltet.", ephemeral: false);
            // User per DM informieren
            await InformUserDM(user, $"Du wurdest für {duration.TotalMinutes} Minuten stummgeschaltet.");
        }

        private async Task HandleKickAsync(IGuildUser user)
        {
            await user.KickAsync("Strafpunkte-Limit erreicht");
            await RespondAsync($"{user.Mention} wurde gekickt.", ephemeral: false);
            // User kann nicht mehr per DM erreicht werden, da er den Server verlassen hat.
            // Alternativ vorher DM schicken:
            try
            {
                await InformUserDM(user, "Du wurdest gekickt, da du das Strafpunkte-Limit erreicht hast.");
            }
            catch { }
        }

        private async Task HandleJailAsync(IGuildUser user)
        {
            // Jail-Funktion noch nicht implementiert.
            // Wir können hier später den JailService aufrufen.
            // Vorläufig:
            //_jailService.JailUserAsync(user, )
            await RespondAsync($"{user.Mention} wird eingesperrt (Funktion noch nicht implementiert).", ephemeral: false);
            await InformUserDM(user, "Du wurdest vorübergehend in den Jail-Channel eingesperrt.");
        }

        private async Task InformUserDM(IGuildUser user, string message)
        {
            try
            {
                var dm = await user.CreateDMChannelAsync();
                await dm.SendMessageAsync(message);
            }
            catch
            {
                // DM konnte nicht zugestellt werden.
            }
        }
    }
}
