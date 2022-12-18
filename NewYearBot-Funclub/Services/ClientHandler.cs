using Discord;
using Discord.WebSocket;
using NewYearBot_Funclub.Utilites;
using Serilog;
using Serilog.Events;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace NewYearBot_Funclub.Services
{
    public class ClientHandler
    {
        private readonly DiscordSocketClient _client;

        public ClientHandler(DiscordSocketClient client)
        {
            _client = client;
        }
        
        public async Task SetStatus()
        {
            await _client.SetGameAsync("снежки", null, ActivityType.Playing);
            await _client.SetStatusAsync(UserStatus.DoNotDisturb);

            Log.Information("{Bot} запущен", _client.CurrentUser.Username);
            await Extensions.LoggerToChannel($"{_client.CurrentUser.Username} запущен", LogEventLevel.Information);
            
            if (Extensions.CurUpTime == DateTimeOffset.FromUnixTimeSeconds(0))
            {
                Extensions.CurUpTime = DateTimeOffset.Now;
            }
            
        }
        

        public async void ClientOnConnected()
        {
            Log.Information("======> Подкючились");
        }

        public async void ClientOnDisconnected(Exception arg)
        {
            Log.Information("======> Отключились");
        }
    }
}