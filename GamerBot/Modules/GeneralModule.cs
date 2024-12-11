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
        [Summary("Listet verfügbare Befehle auf.")]
        public async Task HelpAsync()
        {
            // Später: Dynamisch aus CommandService lesen und je nach User Type filtern
            await ReplyAsync("Verfügbare Befehle: !ping, !help, ... (wird noch erweitert)");
        }
    }
}
