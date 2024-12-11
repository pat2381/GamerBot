using Discord.Interactions;
using GamerBot.Data.Repository;
using GamerBot.Models;
using GamerBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamerBot.Modules
{
    public class LevelInteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly UserRepository _userRepo;
        private readonly Config _config;
        private readonly LevelHelper _levelHelper;

        public LevelInteractionModule(UserRepository userRepo, Config config, LevelHelper levelHelper)
        {
            _userRepo = userRepo;
            _config = config;
            _levelHelper = levelHelper;
        }

        [SlashCommand("rank", "Zeigt dein aktuelles Level und XP an.")]
        public async Task RankAsync()
        {
            try
            {
                await DeferAsync(); // Discord weiß: Antwort kommt gleich

                var user = await _userRepo.GetUserAsync(Context.User.Id, Context.Guild.Id);
                if (user == null)
                {
                    await RespondAsync("Du hast noch keine XP.");
                    return;
                }

                var currentLevel = user.Level;
                var currentXP = user.XP;
                var nextLevel = currentLevel + 1;
                var requiredXP = _levelHelper.GetRequiredXPForLevel(nextLevel);
                var xpToNextLevel = requiredXP - currentXP;

                await RespondAsync($"{Context.User.Mention}, du bist Level {currentLevel} mit {currentXP} XP. " +
                                   $"Du benötigst noch {xpToNextLevel} XP für Level {nextLevel}.");
            }
            catch (Exception ex)
            {
                // Logge den Fehler und antworte ephemeral
                await RespondAsync($"Fehler: {ex.Message}", ephemeral: true);
            }
        }

        [SlashCommand("leaderboard", "Zeigt die Top-User nach XP.")]
        public async Task LeaderboardAsync()
        {
            await DeferAsync(); // Discord weiß: Antwort kommt gleich

            var topUsers = await _userRepo.GetTopUsersAsync(Context.Guild.Id, _config.LeaderboardLimit);

            if (topUsers.Count == 0)
            {
                await RespondAsync("Noch keine Daten vorhanden.");
                return;
            }

            var leaderboardText = string.Join("\n", topUsers.Select((u, i) => $"{i + 1}. <@{u.UserId}> - Level {u.Level} - {u.XP} XP"));
            await FollowupAsync($"**Leaderboard (Top {_config.LeaderboardLimit}):**\n{leaderboardText}");
        }
    }
}
