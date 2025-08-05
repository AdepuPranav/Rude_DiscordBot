using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace ChatBotDiscord.Commands.Echo
{
        
    public class EchoCommand : ModuleBase<SocketCommandContext>
    {
        [Command("echo")]
        [Summary("Echoes back what it said")]
        public async Task ExecuteAsync([Remainder][Summary("A Phrase")] string phrase) {
            if (string.IsNullOrEmpty(phrase)) {
                await ReplyAsync($"Usage: !echo <phrase>");
                return;
            }
            await ReplyAsync(phrase);
            await Context.Message.DeleteAsync();
        }
    }
}
