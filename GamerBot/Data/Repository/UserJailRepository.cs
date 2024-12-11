using GamerBot.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamerBot.Data.Repository
{
    public class UserJailRepository
    {
        private readonly BotDbContext _dbContext;

        public UserJailRepository(BotDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserJailData> GetOrCreateJailDataAsync(ulong userId, ulong guildId)
        {
            var jailData = await _dbContext.UserJail.FirstOrDefaultAsync(j => j.UserId == userId && j.GuildId == guildId);
            if (jailData == null)
            {
                jailData = new UserJailData
                {
                    UserId = userId,
                    GuildId = guildId,
                    IsJailed = false
                };
                _dbContext.UserJail.Add(jailData);
                await _dbContext.SaveChangesAsync();
            }
            return jailData;
        }

        public async Task UpdateJailDataAsync(UserJailData jailData)
        {
            _dbContext.UserJail.Update(jailData);
            await _dbContext.SaveChangesAsync();
        }
    }
}
