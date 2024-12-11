using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GamerBot.Models;

namespace GamerBot.Data
{
    public class BotDbContextFactory : IDesignTimeDbContextFactory<BotDbContext>
    {
        public BotDbContext CreateDbContext(string[] args)
        {
            var json = File.ReadAllText("config.json");
            var config = System.Text.Json.JsonSerializer.Deserialize<Config>(json);

            var optionsBuilder = new DbContextOptionsBuilder<BotDbContext>();
            optionsBuilder.UseSqlite($"Data Source={config?.DatabaseFile}");

            return new BotDbContext(optionsBuilder.Options);
        }
    }
}
