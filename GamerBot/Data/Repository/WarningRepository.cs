using GamerBot.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamerBot.Data.Repository
{
    public class WarningRepository
    {
        private readonly BotDbContext _dbContext;

        public WarningRepository(BotDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<WarningData> GetOrCreateWarningAsync(ulong userId, ulong guildId)
        {
            var warning = await _dbContext.Warnings.FirstOrDefaultAsync(w => w.UserId == userId && w.GuildId == guildId);
            if (warning == null)
            {
                warning = new WarningData
                {
                    UserId = userId,
                    GuildId = guildId,
                    Count = 0
                };
                _dbContext.Warnings.Add(warning);
                await _dbContext.SaveChangesAsync();
            }
            return warning;
        }

        public async Task IncrementWarningAsync(ulong userId, ulong guildId)
        {
            var warning = await GetOrCreateWarningAsync(userId, guildId);
            warning.Count += 1;
            _dbContext.Warnings.Update(warning);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> GetWarningCountAsync(ulong userId, ulong guildId)
        {
            var warning = await _dbContext.Warnings.FirstOrDefaultAsync(w => w.UserId == userId && w.GuildId == guildId);
            return warning?.Count ?? 0;
        }
    }
}
