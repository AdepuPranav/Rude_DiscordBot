using System;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using OllamaSharp;
using OllamaSharp.Models;

namespace ChatBotDiscord.Commands.Ollama
{
    public class OlaCommand : ModuleBase<SocketCommandContext>
    {
        private readonly OllamaApiClient _ollamaClient;

        public OlaCommand(OllamaApiClient ollamaClient)
        {
            _ollamaClient = ollamaClient;
        }

        [Command("Ola")]
        [Summary("Asks Olamma Bot for answers")]
        public async Task ExecuteAsync([Remainder][Summary("A Phrase")] string phrase)
        {
            if (string.IsNullOrEmpty(phrase))
            {
                await ReplyAsync("Usage: !Ola <phrase>");
                return;
            }

            var botResponse = await ReplyAsync("Thu ruk bete...");

            try
            {
                var chat = new Chat(_ollamaClient);
                var responseBuilder = new StringBuilder();

                // Call a dedicated method to handle the streaming response
                await StreamResponseAsync(chat, phrase, responseBuilder);

                string fullResponse = responseBuilder.ToString();

                if (!string.IsNullOrEmpty(fullResponse))
                {
                    var chunks = SplitMessage(fullResponse, 2000);
                    if (chunks.Count > 0)
                    {
                        await botResponse.ModifyAsync(m => m.Content = chunks[0]);

                        // Send the remaining parts as follow-up messages
                        for (int i = 1; i < chunks.Count; i++)
                        {
                            await ReplyAsync(chunks[i]);
                        }
                    }
                }
                else
                {
                    await botResponse.ModifyAsync(m => m.Content = "Ollama did not return a response.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error communicating with Ollama: {ex.Message}");
                await botResponse.ModifyAsync(m => m.Content = $"An error occurred while processing your request: {ex.Message}");
            }
        }

        // A helper method to handle the streaming response robustly
        private async Task StreamResponseAsync(Chat chat, string prompt, StringBuilder responseBuilder)
        {
            await foreach (var streamPart in chat.SendAsync(prompt))
            {
                responseBuilder.Append(streamPart);
            }
        }

        private List<string> SplitMessage(string message, int maxChunkSize)
        {
            var result = new List<string>();

            for (int i = 0; i < message.Length; i += maxChunkSize)
            {
                int length = Math.Min(maxChunkSize, message.Length - i);
                result.Add(message.Substring(i, length));
            }

            return result;
        }
    }
}