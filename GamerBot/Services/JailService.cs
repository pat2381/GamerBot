using Discord.WebSocket;
using Discord;
using GamerBot.Data.Repository;
using GamerBot.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace GamerBot.Services
{
    public class JailService
    {
        private readonly DiscordSocketClient _client;
        private readonly Config _config;
        private readonly UserJailRepository _jailRepo;
        private readonly ILogger<JailService> _logger;
        private readonly IServiceProvider _services;

        // Wir halten Timer in einem Dictionary, um nach Ablauf den User zu entsperren.
        // Key: (guildId,userId)
        private ConcurrentDictionary<(ulong, ulong), Timer> _jailTimers = new ConcurrentDictionary<(ulong, ulong), Timer>();

        public JailService(
            DiscordSocketClient client,
            Config config,
            UserJailRepository jailRepo,
            ILogger<JailService> logger,
            IServiceProvider services)
        {
            _client = client;
            _config = config;
            _jailRepo = jailRepo;
            _logger = logger;
            _services = services;
        }

        /// <summary>
        /// Sperrt einen User für eine bestimmte Dauer ein.
        /// Merkt sich die ursprünglichen Rollen, entfernt sie,
        /// gibt Jail-Rolle, setzt Timer.
        /// </summary>
        public async Task JailUserAsync(IGuildUser user, TimeSpan duration)
        {
            var guild = user.Guild;
            var jailData = await _jailRepo.GetOrCreateJailDataAsync(user.Id, guild.Id);

            if (jailData.IsJailed)
            {
                _logger.LogInformation($"User {user} ist bereits eingesperrt.");
                return;
            }

            // Jail-Rolle finden oder erstellen
            var jailRole = guild.Roles.FirstOrDefault(r => r.Name == _config.JailRoleName);
            if (jailRole == null)
            {
                jailRole = await guild.CreateRoleAsync(_config.JailRoleName,
                    new GuildPermissions(),
                    color: null,
                    isHoisted: false,
                    isMentionable: false);
            }

            // Channels so einstellen, dass nur Jail-Channel zugänglich ist:
            // Dies ist auf Discord-Ebene einzustellen (Channel-Permissions).
            // Wir gehen hier davon aus, dass der Jail-Channel bereits richtig konfiguriert ist,
            // oder das manuell gemacht wurde.

            // Ursprüngliche Rollen sichern (außer @everyone und Jail-Rolle selbst)
            var originalRoles = user.RoleIds
                                    .Where(rid => rid != guild.EveryoneRole.Id && rid != jailRole.Id)
                                    .ToList();

            // Serialisieren
            var originalRolesJson = JsonSerializer.Serialize(originalRoles);

            // Rollen entfernen
            foreach (var rid in originalRoles)
            {
                var role = guild.GetRole(rid);
                if (role != null)
                {
                    await user.RemoveRoleAsync(role);
                }
            }

            // Jail-Rolle zuweisen
            await user.AddRoleAsync(jailRole);

            jailData.IsJailed = true;
            jailData.OriginalRolesJson = originalRolesJson;
            jailData.JailEndTime = DateTimeOffset.UtcNow.Add(duration);

            await _jailRepo.UpdateJailDataAsync(jailData);

            // Timer setzen, um nach Ablauf zu entsperren
            var key = (guild.Id, user.Id);
            var timer = new Timer(async _ =>
            {
                await UnjailUserAsync(user);
            }, null, duration, Timeout.InfiniteTimeSpan);

            _jailTimers[key] = timer;

            _logger.LogInformation($"User {user} wurde für {duration.TotalMinutes} Minuten eingesperrt.");
        }

        /// <summary>
        /// Entsperrt den User nach Ablauf.
        /// Entfernt Jail-Rolle und gibt ursprüngliche Rollen zurück.
        /// </summary>
        public async Task UnjailUserAsync(IGuildUser user)
        {
            var guild = user.Guild;
            var jailData = await _jailRepo.GetOrCreateJailDataAsync(user.Id, guild.Id);

            if (!jailData.IsJailed)
            {
                _logger.LogInformation($"User {user} ist nicht eingesperrt.");
                return;
            }

            // Jail-Rolle entfernen
            var jailRole = guild.Roles.FirstOrDefault(r => r.Name == _config.JailRoleName);
            if (jailRole != null && user.RoleIds.Contains(jailRole.Id))
            {
                await user.RemoveRoleAsync(jailRole);
            }

            // Ursprüngliche Rollen wiederherstellen
            if (!string.IsNullOrEmpty(jailData.OriginalRolesJson))
            {
                var originalRoles = JsonSerializer.Deserialize<ulong[]>(jailData.OriginalRolesJson);
                if (originalRoles != null)
                {
                    foreach (var rid in originalRoles)
                    {
                        var role = guild.GetRole(rid);
                        if (role != null)
                        {
                            await user.AddRoleAsync(role);
                        }
                    }
                }
            }

            jailData.IsJailed = false;
            jailData.OriginalRolesJson = null;
            // JailEndTime kann zurückgesetzt werden, muss aber nicht unbedingt.
            await _jailRepo.UpdateJailDataAsync(jailData);

            // Timer entfernen
            var key = (guild.Id, user.Id);
            if (_jailTimers.TryRemove(key, out var timer))
            {
                timer.Dispose();
            }

            _logger.LogInformation($"User {user} wurde wieder entsperrt.");
        }
    }
}
