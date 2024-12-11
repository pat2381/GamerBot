using Discord.Commands;
using Discord.WebSocket;
using GamerBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GamerBot.Services
{
    public class CommandHandlingService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;
        private readonly Config _config;

        public CommandHandlingService(
            DiscordSocketClient client,
            CommandService commands,
            IServiceProvider services,
            Config config)
        {
            _client = client;
            _commands = commands;
            _services = services;
            _config = config;
        }

        public async Task InitializeAsync()
        {
            // Module laden
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            // Event anmelden
            _client.MessageReceived += HandleCommandAsync;
        }

        private async Task HandleCommandAsync(SocketMessage msg)
        {
            var message = msg as SocketUserMessage;
            if (message == null) return;
            if (message.Author.IsBot) return;

            int argPos = 0;
            if (!(message.HasStringPrefix(_config.Prefix, ref argPos) ||
                  message.HasMentionPrefix(_client.CurrentUser, ref argPos)))
                return;

            var context = new SocketCommandContext(_client, message);
            var result = await _commands.ExecuteAsync(context, argPos, _services);

            if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                await context.Channel.SendMessageAsync(result.ErrorReason);
        }
    }
}
