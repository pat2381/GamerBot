using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamerBot.Modules
{
    public class GeneralInteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("ping", "Antwortet mit Pong!")]
        public async Task PingAsync()
        {
            await RespondAsync("Pong!");
        }
    }
}
