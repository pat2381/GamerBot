using Discord.Commands;
using GamerBot.Data.Repository;
using GamerBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamerBot.Modules
{
    public class LevelModule: ModuleBase<SocketCommandContext>
    {

        private readonly UserRepository _userRepo;
        private readonly Config _config;

        public LevelModule(UserRepository userRepo, Config config)
        {
            _userRepo = userRepo;
            _config = config;
        }

        [Command("rank")]
        [Summary("Zeigt deinen aktuellen Level und XP an.")]
        public async Task RankAsync()
        {
            var user = await _userRepo.GetUserAsync(Context.User.Id, Context.Guild.Id);
            if (user == null)
            {
                await ReplyAsync("Du hast noch keine XP.");
                return;
            }

            // Nächste Level XP berechnen
            var nextLevel = user.Level + 1;
            int requiredXP = nextLevel * 100; // oder besser eine Funktion aufrufen, wie im XPService
            int currentXP = user.XP;
            int currentLevel = user.Level;

            await ReplyAsync($"{Context.User.Mention}, du bist Level {currentLevel} mit {currentXP} XP. Du benötigst {requiredXP - currentXP} XP für Level {nextLevel}.");
        }

        [Command("leaderboard")]
        [Summary("Zeigt die Top-User nach XP.")]
        public async Task LeaderboardAsync()
        {
            var topUsers = await _userRepo.GetTopUsersAsync(Context.Guild.Id, _config.LeaderboardLimit);

            if (topUsers.Count == 0)
            {
                await ReplyAsync("Noch keine Daten vorhanden.");
                return;
            }

            var leaderboardText = string.Join("\n", topUsers.Select((u, i) => $"{i + 1}. <@{u.UserId}> - Level {u.Level} - {u.XP} XP"));
            await ReplyAsync($"**Leaderboard (Top {_config.LeaderboardLimit}):**\n{leaderboardText}");
        }
    }
}
