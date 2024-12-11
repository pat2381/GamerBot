using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamerBot.Modules
{
    internal class GeneralModule : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        [Summary("Antwortet mit 'Pong!'")]
        public async Task PingAsync()
        {
            await ReplyAsync("Pong!");
        }

        [Command("help")]
        [Summary("Listet verfügbare Befehle auf. Mods sehen zusätzliche Befehle.")]
        public async Task HelpAsync()
        {
            // Primitives Beispiel: Prüfe, ob der Nutzer ein Mod ist, indem wir auf die Berechtigung KickMembers prüfen
            var guildUser = Context.User as Discord.WebSocket.SocketGuildUser;
            bool isMod = guildUser != null && guildUser.GuildPermissions.KickMembers;

            // Allgemeine Befehle
            var helpMessage = "**Verfügbare Befehle:**\n" +
                              "`!ping` - Prüfe ob der Bot reagiert\n" +
                              "`!rank` - Zeige deinen aktuellen Level und XP\n" +
                              "`!leaderboard` - Zeige die Top 10 Nutzer";

            if (isMod)
            {
                helpMessage += "\n\n**Moderationsbefehle:**\n" +
                               "`!warn @User <Grund>` - Warne einen Nutzer\n" +
                               "`!mute @User <Dauer>` - Mute einen Nutzer\n" +
                               "`!kick @User <Grund>` - Kicke einen Nutzer\n" +
                               // weitere Mod-Befehle hier einfügen
                               "`!backup` - Erstelle ein Backup der Datenbank (falls implementiert)";
            }

            await ReplyAsync(helpMessage);
        }
    }
}
