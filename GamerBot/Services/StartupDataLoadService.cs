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
    public class StartupDataLoadService
    {
        private readonly ForbiddenWordsRepository _forbiddenWordsRepo;
        private readonly Config _config;
        private readonly ILogger<StartupDataLoadService> _logger;

        public StartupDataLoadService(ForbiddenWordsRepository forbiddenWordsRepo, Config config, ILogger<StartupDataLoadService> logger)
        {
            _forbiddenWordsRepo = forbiddenWordsRepo;
            _config = config;
            _logger = logger;
        }

        public async Task LoadForbiddenWordsAsync()
        {
            var filePath = _config.ForbiddenWordsFilePath;
            if (string.IsNullOrWhiteSpace(filePath))
            {
                _logger.LogWarning("Kein ForbiddenWordsFilePath in der Config angegeben.");
                return;
            }

            if (!File.Exists(filePath))
            {
                _logger.LogWarning($"Forbidden-Words-Datei nicht gefunden: {filePath}");
                return;
            }

            var lines = await File.ReadAllLinesAsync(filePath);
            int addedCount = 0;

            foreach (var line in lines)
            {
                var word = line.Trim();
                if (!string.IsNullOrEmpty(word))
                {
                    bool exists = await _forbiddenWordsRepo.WordExistsAsync(word);
                    if (!exists)
                    {
                        await _forbiddenWordsRepo.AddWordAsync(word);
                        addedCount++;
                    }
                }
            }

            if (addedCount > 0)
                _logger.LogInformation($"{addedCount} neue verbotene Wörter aus der Datei hinzugefügt.");
            else
                _logger.LogInformation("Keine neuen verbotenen Wörter gefunden.");
        }
    }
}
