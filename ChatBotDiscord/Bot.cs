using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using OllamaSharp;

namespace ChatBotDiscord
{
    public class Bot : IBot
    {
        private readonly ILogger<Bot> _logger;
        private readonly IConfiguration _configuration;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private IServiceProvider? _serviceProvider;

        // The constructor now receives all dependencies via injection
        public Bot(ILogger<Bot> logger, IConfiguration configuration, DiscordSocketClient client, CommandService commands)
        {
            _logger = logger;
            _configuration = configuration;
            _client = client;
            _commands = commands;
        }

        public async Task StartAsync(IServiceProvider services)
        {
            _serviceProvider = services;

            string discordToken = _configuration["DiscordToken"] ?? throw new Exception("Missing Discord Token");
            _logger.LogInformation($"Starting up");

            // The CommandService uses the provided ServiceProvider to inject dependencies into command modules
            await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), _serviceProvider);

            _client.MessageReceived += HandleCommandAsync;

            await _client.LoginAsync(TokenType.Bot, discordToken);
            await _client.StartAsync();
        }

        public async Task StopAsync()
        {
            _logger.LogInformation("Shutting down");
            if (_client != null)
            {
                await _client.LogoutAsync();
                await _client.StopAsync();
            }
        }

        public async Task HandleCommandAsync(SocketMessage arg)
        {
            // Ensure the message is from a user and not a bot
            if (arg is not SocketUserMessage message || message.Author.IsBot)
            {
                return;
            }

            _logger.LogInformation($"{DateTime.Now.ToShortTimeString()} - {message.Author} : {message.Content}");

            int position = 0;
            // Check for the command prefix or mention
            if (!message.HasCharPrefix('!', ref position) && !message.HasMentionPrefix(_client.CurrentUser, ref position))
            {
                return;
            }

            var context = new SocketCommandContext(_client, message);
            var result = await _commands.ExecuteAsync(context, position, _serviceProvider);

            if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
            {
                _logger.LogError($"Command execution failed: {result.ErrorReason}");
                await context.Channel.SendMessageAsync($"Error: {result.ErrorReason}");
            }
        }
    }
}
