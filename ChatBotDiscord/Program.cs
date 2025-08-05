using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OllamaSharp;

namespace ChatBotDiscord
{
    class Program
    {
        private static void Main(string[] args) =>
            MainAsync(args).GetAwaiter().GetResult();

        private static async Task MainAsync(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets(Assembly.GetExecutingAssembly())
                .Build();

            // Set up dependency injection for all necessary services
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddLogging(options =>
            {
                options.ClearProviders();
                options.AddConsole();
            });

            serviceCollection.AddSingleton<IConfiguration>(configuration);
            serviceCollection.AddSingleton<IBot, Bot>();

            // Register Discord.Net services
            serviceCollection.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            }));
            serviceCollection.AddSingleton(new CommandService());

            // Register OllamaApiClient as a singleton
            // This ensures all commands share the same client instance
            serviceCollection.AddSingleton(new OllamaApiClient("http://localhost:11434")
            {
                SelectedModel = "myllama3.1" // Set your preferred model here
            });

            var serviceProvider = serviceCollection.BuildServiceProvider();

            try
            {
                IBot bot = serviceProvider.GetRequiredService<IBot>();
                await bot.StartAsync(serviceProvider);

                Console.WriteLine("Bot is running. Press 'Q' to quit.");

                do
                {
                    var keyInfo = Console.ReadKey();
                    if (keyInfo.Key == ConsoleKey.Q)
                    {
                        await bot.StopAsync();
                        return;
                    }
                } while (true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unhandled exception occurred: {ex.Message}");
                Environment.Exit(-1);
            }
        }
    }
}