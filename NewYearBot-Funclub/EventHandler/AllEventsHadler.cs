using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using NewYearBot_Funclub.Services;
using Serilog;
#pragma warning disable CS1998

namespace Discordbot.EventHandler;

public class AllEventsHadler
{
    private readonly DiscordSocketClient _client;
    private readonly ClientHandler _clientHandler;
    private readonly SlashCommandHandler _slashCommandHandler;
    private readonly SnowesHandler _snowesHandler;
    private readonly PaintHandler _paintHandler;

    public AllEventsHadler(DiscordSocketClient client, ClientHandler clientHandler, 
        SlashCommandHandler slashCommandHandler, SnowesHandler snowesHandler, PaintHandler paintHandler)
    {
        _client = client;
        _clientHandler = clientHandler;
        _slashCommandHandler = slashCommandHandler;
        _snowesHandler = snowesHandler;
        _paintHandler = paintHandler;
    }
    
    
    public async Task InitializeAsync ( )
    {
        
        //_client.GuildStickerCreated;
        //_client.GuildStickerUpdated;
        //_client.GuildStickerDeleted += ;
        
        _client.MessageReceived += ClientOnMessageReceived;
        
        //_client.UserUpdated += ClientOnUserUpdated;
        
        _client.UserVoiceStateUpdated += ClientOnUserVoiceStateUpdated;
        
        _client.Ready += ClientOnReady;
        _client.Disconnected += ClientOnDisconnected;
        _client.Connected += ClientOnConnected;
        
        _client.InteractionCreated += ClientOnInteractionCreated;
    }

    
    private Task ClientOnConnected()
    {
        _ = Task.Run(async () => _clientHandler.ClientOnConnected());
        return Task.CompletedTask;
    }

    private Task ClientOnDisconnected(Exception arg)
    {
        _ = Task.Run(async () => _clientHandler.ClientOnDisconnected(arg));
        return Task.CompletedTask;
    }
    
    private Task ClientOnInteractionCreated(SocketInteraction arg)
    {

        //Log.Information("Command started: {commandType}, {commandName} ({GuildId}) | {User} ({userId})", arg.Type, arg.Id, arg.GuildId ?? 0, arg.User.Username, arg.User.Id);
        _ = Task.Run(async () => _slashCommandHandler.HandleInteraction(arg));
        _ = Task.Run(async () => _paintHandler.HandleInteraction(arg));
        return Task.CompletedTask;
    }
    

    private Task ClientOnReady()
    {
        _ = Task.Run(async () => _clientHandler.SetStatus());
        return Task.CompletedTask;
    }

    private Task ClientOnMessageReceived(SocketMessage arg)
    {
        //Log.Information("Message send {userNaem} ({userId})", arg.Author.Username, arg.Author.Id);
        _ = Task.Run(async () => _snowesHandler.XpTextSystem(arg));
        return Task.CompletedTask;
    }
    

    private Task ClientOnUserVoiceStateUpdated(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
    {
        //Log.Information("Voice state updated {userNaem} ({userId})", arg1.Username, arg1.Id);
        _ = Task.Run(async () => _snowesHandler.XpVoiceSystem(arg1, arg2, arg3));
        return Task.CompletedTask;
    }
}