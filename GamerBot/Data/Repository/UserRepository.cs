using GamerBot.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamerBot.Data.Repository
{
    public class UserRepository
    {
        private readonly BotDbContext _dbContext;

        public UserRepository(BotDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User> GetOrCreateUserAsync(ulong userId, ulong guildId)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.GuildId == guildId);
            if (user == null)
            {
                user = new User
                {
                    UserId = userId,
                    GuildId = guildId,
                    XP = 0,
                    Level = 0
                };
                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();
            }
            return user;
        }

        public async Task UpdateUserAsync(User user)
        {
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<User?> GetUserAsync(ulong userId, ulong guildId)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.GuildId == guildId);
        }

        // Optional: Methoden für Leaderboard, z. B.
        public async Task<List<User>> GetTopUsersAsync(ulong guildId, int limit)
        {
            return await _dbContext.Users
                .Where(u => u.GuildId == guildId)
                .OrderByDescending(u => u.XP)
                .Take(limit)
                .ToListAsync();
        }
    }
}
