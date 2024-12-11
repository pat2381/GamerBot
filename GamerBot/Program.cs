using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GamerBot.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Windows.Input;
using GamerBot.Services;
using GamerBot.Data;
using Microsoft.EntityFrameworkCore;
using GamerBot.Data.Repository;

namespace GamerBot;
public class Program
{
    public static Config? Config { get; private set; }

    private DiscordSocketClient? _client;
    private CommandService? CommandService;
    public static async Task Main(string[] args)
    {
        // Config laden
        var jsonString = await File.ReadAllTextAsync("config.json");
        Config = JsonSerializer.Deserialize<Config>(jsonString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Host erstellen
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
            })
            .ConfigureServices((context, services) =>
            {
                // Config als Singleton verfügbar machen
                services.AddSingleton(Config!);

                // Discord Client und Services
                services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Info
                }));
                services.AddSingleton(new CommandService());


                // EF Core / Datenbank
                // Hier fügen wir dann DbContext hinzu (siehe unten EF-Integration)

                services.AddDbContext<BotDbContext>(options =>
                {
                    options.UseSqlite($"Data Source={Program.Config?.DatabaseFile}");
                });

                // Andere Services (z. B. CommandHandlingService, InteractionHandlingService)
                // Diese Services erhalten den Client und CommandService durch DI

                services.AddSingleton<UserRepository>();
                services.AddSingleton<XPService>();

                // StartService ist ein Service, der beim Start ausgeführt wird, um Bot zu initialisieren.
                services.AddHostedService<BotStartupService>();
            })
            .Build();

        await host.RunAsync();
    }

}
