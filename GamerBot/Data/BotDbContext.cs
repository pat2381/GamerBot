using GamerBot.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamerBot.Data
{
    public class BotDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<WarningData> Warnings { get; set; }
        public DbSet<ForbiddenWord> ForbiddenWords { get; set; }
        public DbSet<UserPenalty> UserPenalties { get; set; }
        public DbSet<UserJailData> UserJail { get; set; }


        private string _dbPath;

        public BotDbContext(string dbPath)
        {
            _dbPath = dbPath;
        }

        // Konstruktor für DI
        public BotDbContext(DbContextOptions<BotDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured && !string.IsNullOrEmpty(_dbPath))
            {
                optionsBuilder.UseSqlite($"Data Source={_dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Falls spezielle Konfiguration nötig ist
        }
    }
}
