using GamerBot.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamerBot.Data.Repository
{
    public class UserPenaltyRepository
    {
        private readonly BotDbContext _dbContext;

        public UserPenaltyRepository(BotDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserPenalty> GetOrCreatePenaltyAsync(ulong userId, ulong guildId)
        {
            var penalty = await _dbContext.UserPenalties.FirstOrDefaultAsync(p => p.UserId == userId && p.GuildId == guildId);
            if (penalty == null)
            {
                penalty = new UserPenalty
                {
                    UserId = userId,
                    GuildId = guildId,
                    TotalPoints = 0,
                    OffenseCount = 0
                };
                _dbContext.UserPenalties.Add(penalty);
                await _dbContext.SaveChangesAsync();
            }
            return penalty;
        }

        public async Task UpdatePenaltyAsync(UserPenalty penalty)
        {
            _dbContext.UserPenalties.Update(penalty);
            await _dbContext.SaveChangesAsync();
        }
    }
}
