using Discord.WebSocket;
using Discord;
using GamerBot.Data.Repository;
using GamerBot.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamerBot.Services
{
    public class ModerationService
    {
        private readonly DiscordSocketClient _client;
        private readonly Config _config;
        private readonly ForbiddenWordsRepository _forbiddenWordsRepo;
        private readonly PenaltyService _penaltyService;
        private readonly ILogger<ModerationService> _logger;

        public ModerationService(
            DiscordSocketClient client,
            Config config,
            ForbiddenWordsRepository forbiddenWordsRepo,
            PenaltyService penaltyService,
            ILogger<ModerationService> logger)
        {
            _client = client;
            _config = config;
            _forbiddenWordsRepo = forbiddenWordsRepo;
            _penaltyService = penaltyService;
            _logger = logger;
        }

        public void Initialize()
        {
            _client.MessageReceived += OnMessageReceived;
        }

        private async Task OnMessageReceived(SocketMessage msg)
        {
            // Keine Bots moderieren
            if (msg.Author.IsBot) return;

            if (msg.Channel is not SocketTextChannel textChannel)
                return;

            var guild = textChannel.Guild;
            var guildId = guild.Id;
            var userId = msg.Author.Id;
            var content = msg.Content;

            // Verbotene Wörter laden
            var forbiddenWords = await _forbiddenWordsRepo.GetAllWordsAsync();
            if (forbiddenWords.Count == 0) return; // Keine verbotenen Wörter definiert

            // Verbote Wörter zählen und zensieren
            var (forbiddenCount, censoredMessage) = CheckAndCensorMessage(content, forbiddenWords, _config.CensorString);

            if (forbiddenCount > 0)
            {
                // Nachricht löschen
                await msg.DeleteAsync();

                // Zensierte Nachricht posten
                // Hinweis: Wir erwähnen den User, damit klar ist, von wem die ursprüngliche Nachricht war.
                var alertMsg = $"{msg.Author.Mention} hat etwas Unangebrachtes geschrieben:\n{censoredMessage}";
                await textChannel.SendMessageAsync(alertMsg);

                // Strafpunkte vergeben
                var penaltyResult = await _penaltyService.AddPenaltyAsync(guildId, userId, forbiddenCount);

                // User per DM informieren
                await InformUserDMAsync(msg.Author, forbiddenCount, penaltyResult.AddedPoints, penaltyResult.NewTotalPoints);

                // Prüfen, ob Max erreicht
                if (penaltyResult.ReachedMaxPoints)
                {
                    await NotifyModsAsync(guild, msg.Author, penaltyResult);
                }

                // Da diese Nachricht bereits als Verstoß gehandhabt wurde, kein weiterer Flow nötig
                return;
            }

            // Kein verbotenes Wort gefunden, normaler Flow geht weiter
        }

        private (int forbiddenCount, string censoredMessage) CheckAndCensorMessage(string content, List<string> forbiddenWords, string censor)
        {
            var words = content.Split(' ');
            int forbiddenCount = 0;
            for (int i = 0; i < words.Length; i++)
            {
                var wLower = words[i].ToLowerInvariant();
                // Prüfen, ob dieses Wort in der Liste steht
                if (forbiddenWords.Any(fw => wLower.Contains(fw.ToLowerInvariant())))
                {
                    forbiddenCount++;
                    // Komplette Wort zensieren
                    words[i] = censor;
                }
            }

            return (forbiddenCount, string.Join(' ', words));
        }

        private async Task InformUserDMAsync(IUser user, int forbiddenCount, int addedPoints, int newTotalPoints)
        {
            var dmChannel = await user.CreateDMChannelAsync();
            await dmChannel.SendMessageAsync($"Deine Nachricht wurde zensiert, da sie {forbiddenCount} verbotene Wörter enthielt." +
                                             $"\nDu hast dafür {addedPoints} Strafpunkte erhalten. Insgesamt hast du jetzt {newTotalPoints} Strafpunkte.");
        }

        private async Task NotifyModsAsync(SocketGuild guild, IUser user, PenaltyResult penaltyResult)
        {
            if (_config.ModerationChannelId == 0)
            {
                _logger.LogWarning("ModerationChannelId ist nicht gesetzt, kann Mods nicht benachrichtigen.");
                return;
            }

            var modChannel = guild.GetTextChannel(_config.ModerationChannelId);
            if (modChannel == null)
            {
                _logger.LogWarning($"Moderationschannel mit ID {_config.ModerationChannelId} wurde nicht gefunden.");
                return;
            }

            // Nachricht an Mods
            // Hier werden später Buttons angefügt, vorerst nur Text
            var msg = $"{user.Mention} hat das Strafpunktlimit erreicht!\n" +
                      $"Verstöße: {penaltyResult.OffenseCount}, Gesamtpunkte: {penaltyResult.NewTotalPoints}.\n" +
                      "Wähle eine Maßnahme (z. B. Timeout, Kick oder Einsperren).";

            var builder = new ComponentBuilder()
                    .WithButton("Timeout", customId: $"setduration:timeout:{penaltyResult.UserId}", style: ButtonStyle.Primary)
                    .WithButton("Kick", customId: $"action:kick:{penaltyResult.UserId}", style: ButtonStyle.Danger)
                    .WithButton("Jail", customId: $"setduration:jail:{penaltyResult.UserId}", style: ButtonStyle.Secondary);


            await modChannel.SendMessageAsync(msg, components: builder.Build());
        }
    }
}
