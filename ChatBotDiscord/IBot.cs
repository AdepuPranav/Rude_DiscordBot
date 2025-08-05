using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace ChatBotDiscord
{
    public interface IBot
    {
        Task StartAsync(IServiceProvider services);
        Task StopAsync();
    }
}
