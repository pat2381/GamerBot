using Discord;
using Discord.Interactions;
using GamerBot.Data.Repository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamerBot.Modules
{
    [Group("words", "Verwaltung der verbotenen Wörter")]
    public class ModerationInteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ForbiddenWordsRepository _forbiddenWordsRepo;
        private readonly ILogger<ModerationInteractionModule> _logger;

        public ModerationInteractionModule(ForbiddenWordsRepository forbiddenWordsRepo, ILogger<ModerationInteractionModule> logger)
        {
            _forbiddenWordsRepo = forbiddenWordsRepo;
            _logger = logger;
        }

        [SlashCommand("add", "Fügt ein verbotenes Wort hinzu")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task AddWordAsync([Summary("wort", "Das hinzuzufügende Wort")] string word)
        {
            word = word.Trim();
            if (string.IsNullOrWhiteSpace(word))
            {
                await RespondAsync("Bitte ein gültiges Wort angeben.", ephemeral: true);
                return;
            }

            bool exists = await _forbiddenWordsRepo.WordExistsAsync(word);
            if (exists)
            {
                await RespondAsync($"Das Wort `{word}` ist bereits in der Liste.", ephemeral: true);
            }
            else
            {
                await _forbiddenWordsRepo.AddWordAsync(word);
                await RespondAsync($"Das Wort `{word}` wurde hinzugefügt.", ephemeral: true);
            }
        }

        [SlashCommand("remove", "Entfernt ein verbotenes Wort")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task RemoveWordAsync([Summary("wort", "Das zu entfernende Wort")] string word)
        {
            word = word.Trim();
            bool exists = await _forbiddenWordsRepo.WordExistsAsync(word);
            if (!exists)
            {
                await RespondAsync($"Das Wort `{word}` wurde nicht gefunden.", ephemeral: true);
            }
            else
            {
                await _forbiddenWordsRepo.RemoveWordAsync(word);
                await RespondAsync($"Das Wort `{word}` wurde entfernt.", ephemeral: true);
            }
        }

        [SlashCommand("list", "Listet alle verbotenen Wörter auf")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task ListWordsAsync()
        {
            var words = await _forbiddenWordsRepo.GetAllWordsAsync();
            if (words.Count == 0)
            {
                await RespondAsync("Es sind keine verbotenen Wörter in der Liste.", ephemeral: true);
            }
            else
            {
                var wordList = string.Join(", ", words.Select(w => $"`{w}`"));
                await RespondAsync($"Verbotene Wörter:\n{wordList}", ephemeral: true);
            }
        }

    }
}
