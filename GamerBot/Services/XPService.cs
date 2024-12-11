using Discord.WebSocket;
using Discord;
using GamerBot.Data.Repository;
using GamerBot.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GamerBot.Services
{
    public class XPService
    {
        private readonly DiscordSocketClient _client;
        private readonly Config? _config;
        private readonly UserRepository _userRepo;
        private readonly ILogger<XPService> _logger;

        public XPService(DiscordSocketClient client, Config config, UserRepository userRepo, ILogger<XPService> logger)
        {
            _client = client;
            _config = config;
            _userRepo = userRepo;
            _logger = logger;
        }

        public void Initialize()
        {
            _client.MessageReceived += OnMessageReceived;
        }

        private async Task OnMessageReceived(SocketMessage message)
        {
            // Keine Bots
            if (message.Author.IsBot) return;

            // Nur Guild-Messages zählen
            if (message.Channel is not SocketTextChannel textChannel)
                return;

            var guildId = textChannel.Guild.Id;
            var userId = message.Author.Id;

            // Userdaten laden oder erstellen
            var user = await _userRepo.GetOrCreateUserAsync(userId, guildId);

            // XP vergeben
            user.XP += _config!.XPPerMessage;

            // Level-Berechnung
            var nextLevel = user.Level + 1;
            var requiredXP = GetRequiredXPForLevel(nextLevel);

            if (user.XP >= requiredXP)
            {
                // Level steigt an
                user.Level = nextLevel;
                await textChannel.SendMessageAsync($"{message.Author.Mention}, du hast Level {user.Level} erreicht!");

                // Rollenvergabe prüfen
                await CheckAndAssignRoleAsync(textChannel.Guild, message.Author as IGuildUser, user.Level);
            }

            // Änderungen speichern
            await _userRepo.UpdateUserAsync(user);
        }

        private int GetRequiredXPForLevel(int level)
        {
            // Beispielhafte Interpretation der LevelCurve:
            // Wenn Config z.B. "n*100" ist, dann required XP = level * 100
            // Man könnte auch einen Parser schreiben, aber hier halten wir es einfach.
            // Annahme: LevelCurve ist ein String im Format "n*100"
            // Dann machen wir einfach level * 100.

            // Für komplexere Formeln könntest du hier einen Parser einbauen.
            // Aktuell hartkodieren wir einfach einen Zusammenhang:
            if (_config!.LevelCurve!.Contains("n*"))
            {
                var match = Regex.Match(_config.LevelCurve, @"n\*(\d+)");
                if (match.Success && int.TryParse(match.Groups[1].Value, out int factor))
                {
                    return level * factor;
                }
            }

            // Fallback: falls nichts lesbar ist, einfach level * 100
            return level * 100;
        }

        private async Task CheckAndAssignRoleAsync(SocketGuild guild, IGuildUser user, int level)
        {
            if (_config!.LevelRoles == null || user == null) return;

            foreach (var entry in _config.LevelRoles)
            {
                if (level == entry.Key)
                {
                    var roleName = entry.Value;
                    var role = guild.Roles.FirstOrDefault(r => r.Name == roleName);

                    // Wenn Rolle nicht existiert, erstellen
                    if (role == null)
                    {
                        role = await guild.CreateRoleAsync(roleName, null, null, false, null);
                    }

                    // Rolle dem User geben
                    if (!user.RoleIds.Contains(role.Id))
                    {
                        await user.AddRoleAsync(role);
                    }
                }
            }
        }
    }
}
