using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamerBot.Modules
{
    public class ModerationModule: ModuleBase<SocketCommandContext>
    {
        [Command("kick")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task KickAsync(IGuildUser user, [Remainder] string reason = "Kein Grund angegeben")
        {
            await user.KickAsync(reason);
            await ReplyAsync($"{user.Mention} wurde gekickt. Grund: {reason}");
        }

        [Command("ban")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task BanAsync(IGuildUser user, [Remainder] string reason = "Kein Grund angegeben")
        {
            await user.BanAsync(0, reason);
            await ReplyAsync($"{user.Mention} wurde gebannt. Grund: {reason}");
        }

        [Command("timeout")]
        [RequireUserPermission(GuildPermission.ModerateMembers)]
        public async Task TimeoutAsync(IGuildUser user, int minutes, [Remainder] string reason = "Kein Grund angegeben")
        {
            var duration = TimeSpan.FromMinutes(minutes);
            await user.SetTimeOutAsync(duration);
            await ReplyAsync($"{user.Mention} wurde für {minutes} Minuten stummgeschaltet. Grund: {reason}");
        }
    }
}
