using GamerBot.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamerBot.Data.Repository
{
    public class ForbiddenWordsRepository
    {
        private readonly BotDbContext _dbContext;

        public ForbiddenWordsRepository(BotDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<string>> GetAllWordsAsync()
        {
            return await _dbContext.ForbiddenWords
                .Select(w => w.Word)
                .ToListAsync();
        }

        public async Task<bool> WordExistsAsync(string word)
        {
            return await _dbContext.ForbiddenWords.AnyAsync(w => w.Word == word);
        }

        public async Task AddWordAsync(string word)
        {
            if (!await WordExistsAsync(word))
            {
                _dbContext.ForbiddenWords.Add(new ForbiddenWord { Word = word });
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task RemoveWordAsync(string word)
        {
            var fw = await _dbContext.ForbiddenWords.FirstOrDefaultAsync(w => w.Word == word);
            if (fw != null)
            {
                _dbContext.ForbiddenWords.Remove(fw);
                await _dbContext.SaveChangesAsync();
            }
        }

    }
}
