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
    public class PenaltyService
    {
        private readonly UserPenaltyRepository _penaltyRepo;
        private readonly Config _config;
        private readonly ILogger<PenaltyService> _logger;

        public PenaltyService(UserPenaltyRepository penaltyRepo, Config config, ILogger<PenaltyService> logger)
        {
            _penaltyRepo = penaltyRepo;
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// Berechnet die Strafpunkte für eine Nachricht mit verbotenen Wörtern.
        /// </summary>
        /// <param name="guildId">Guild-Id</param>
        /// <param name="userId">User-Id</param>
        /// <param name="forbiddenWordCount">Anzahl verbotener Wörter in dieser Nachricht</param>
        /// <returns>Ein Ergebnisobjekt mit neuen Punkten, OffenseCount und Info ob max. Punkte erreicht</returns>
        public async Task<PenaltyResult> AddPenaltyAsync(ulong guildId, ulong userId, int forbiddenWordCount)
        {
            var penalty = await _penaltyRepo.GetOrCreatePenaltyAsync(userId, guildId);

            // Grundstrafe: 1 + 2 + 3 ... für jedes Wort
            // Formel: Summe der ersten n Zahlen = n*(n+1)/2
            // z.B. 3 verbotene Wörter = 1+2+3 = 6 Punkte Basis
            int basePoints = forbiddenWordCount * (forbiddenWordCount + 1) / 2;

            // Multiplikator basierend auf OffenseCount
            double multiplier = GetMultiplier(penalty.OffenseCount);

            int addedPoints = (int)Math.Ceiling(basePoints * multiplier);

            // Update Daten:
            penalty.TotalPoints += addedPoints;
            penalty.OffenseCount += 1;

            await _penaltyRepo.UpdatePenaltyAsync(penalty);

            bool reachedMax = penalty.TotalPoints >= _config.MaxPenaltyPoints;

            return new PenaltyResult
            {
                UserId = userId,
                GuildId = guildId,
                NewTotalPoints = penalty.TotalPoints,
                OffenseCount = penalty.OffenseCount,
                AddedPoints = addedPoints,
                ReachedMaxPoints = reachedMax
            };
        }

        /// <summary>
        /// Ermittelt den Multiplikator aus der Config auf Basis der Anzahl vorheriger Verstöße.
        /// </summary>
        private double GetMultiplier(int offenseCount)
        {
            // _config.PenaltyMultipliers ist ein Dictionary<int, double>
            // Schlüssel: Anzahl vorheriger Verstöße-Schwelle
            // Wert: Multiplikator

            // Idee: Finde den höchsten Schlüssel, der <= offenseCount ist.
            // Beispiel:
            //  "0": 1.0
            //  "1": 1.1
            //  "3": 1.5
            //  "5": 2.0
            //
            // Bei offenseCount=4 gilt dann "3":1.5 da 3 <=4 und 5 >4

            if (_config.PenaltyMultipliers == null || _config.PenaltyMultipliers.Count == 0)
                return 1.0; // Fallback

            // Sortiere Keys
            var keys = _config.PenaltyMultipliers.Keys.OrderBy(k => k).ToList();
            double multiplier = 1.0;

            foreach (var key in keys)
            {
                if (offenseCount >= key)
                    multiplier = _config.PenaltyMultipliers[key];
                else
                    break;
            }

            return multiplier;
        }
    }
}
