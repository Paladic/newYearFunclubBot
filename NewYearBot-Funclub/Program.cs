using System.Diagnostics;
using System.Net.Mime;
using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Webhook;
using Discord.WebSocket;
using Discordbot.EventHandler;
using Fergun.Interactive;
using Infrastructure.Context;
using Infrastructure.DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NewYearBot_Funclub.Commands;
using NewYearBot_Funclub.Services;
using NewYearBot_Funclub.Utilites;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace NewYearBot_Funclub
{
    class Program {
        
     public static DiscordSocketClient client;
     public static DiscordWebhookClient Logger;
     public static DiscordWebhookClient MapLogger;
     public static IConfiguration config;
     
        static void Main ()
        {
            // One of the more flexable ways to access the configuration data is to use the Microsoft's Configuration model,
            // this way we can avoid hard coding the environment secrets. I opted to use the Json and environment variable providers here.
            config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .Build();

            Logger = new DiscordWebhookClient(config["LoggerWebhook"]);
            MapLogger = new DiscordWebhookClient(config["MapLoggerWebhook"]);

            DiscordSocketConfig cf = new DiscordSocketConfig
            {
                 MessageCacheSize = 2000, 
                 AlwaysDownloadUsers = true, 
                 LogLevel = LogSeverity.Info,
                 GatewayIntents =
                     GatewayIntents.All

            };

            RunAsync(cf, config).GetAwaiter().GetResult();
        }
        
         static async Task RunAsync (DiscordSocketConfig cf, IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.Console(theme: AnsiConsoleTheme.Literate)
                .CreateLogger();
            
            // Dependency injection is a key part of the Interactions framework but it needs to be disposed at the end of the app's lifetime.
            using var services = ConfigureServices(cf, configuration);

            client = services.GetRequiredService<DiscordSocketClient>();
            var commands = services.GetRequiredService<InteractionService>();
            client.Log += LogAsync;
            commands.Log += LogAsync;

            // Slash Commands and Context Commands are can be automatically registered, but this process needs to happen after the client enters the READY state.
            // Since Global Commands take around 1 hour to register, we should use a test guild to instantly update and test our commands. To determine the method we should
            // register the commands with, we can check whether we are in a DEBUG environment and if we are, we can register the commands to a predetermined test guild.
            client.Ready += async ( ) =>
            {
                await commands.RegisterCommandsToGuildAsync(UInt64.Parse(config["GuildId"]), true);
                //await commands.RegisterCommandsGloballyAsync(true);
            };

            await services.GetRequiredService<AllEventsHadler>().InitializeAsync();
            // Here we can initialize the service that will register and execute our commands
            await services.GetRequiredService<SlashCommandHandler>().InitializeAsync();
            await services.GetRequiredService<PaintHandler>().InitializeAsync();
            // Bot token can be provided from the Configuration object we set up earlier
            await client.LoginAsync(TokenType.Bot, configuration["BotToken"]);
            
            await client.StartAsync();

            await Task.Delay(Timeout.Infinite);
        }

        //[SuppressMessage("ReSharper", "LocalizableElement")]
        public static Task LogAsync(LogMessage message)
        {
            _ = Task.Run(async () =>
            {
                var severity = message.Severity switch
                {
                    LogSeverity.Critical => LogEventLevel.Fatal,
                    LogSeverity.Error => LogEventLevel.Error,
                    LogSeverity.Warning => LogEventLevel.Warning,
                    LogSeverity.Info => LogEventLevel.Information,
                    LogSeverity.Verbose => LogEventLevel.Verbose,
                    LogSeverity.Debug => LogEventLevel.Debug,
                    _ => LogEventLevel.Information
                };
                // Console.WriteLine($"===============\n" + $"{message.Exception.Message}\n"); Discord.WebSocket.GatewayReconnectException: Server missed last heartbeat
                //  Console.WriteLine($"===============\n" + $"{message.Exception.StackTrace}\n");  at Discord.ConnectionManager.<>c__DisplayClass29_0.<<StartAsync>b__0>d.MoveNext()

                // Server missed last heartbeat
                //Messages must be younger than two weeks old. (Parameter 'MessageIds')
                Log.Write(severity, message.Exception, "[{Source}] {Message}", message.Source, message.Message);
                
                if ((message.Exception != null && message.Exception.Message != null) &&
                    ((message.Exception.Message == "Server missed last heartbeat" ||
                      message.Exception.Message.StartsWith("Server missed last heartbeat"))))
                {
                    //Log.Write(severity, message.Exception, "[{Source}] {Message}", message.Source, message.Message);
                    
                    //GC.Collect();
                    //GC.WaitForPendingFinalizers();
                    Console.Clear();
                    Log.Information("=============================================");
                    Log.Information("Начат перезапуск бота");
                    Log.Information("=============================================");
                    
                    var path = Assembly.GetExecutingAssembly().Location.Split(".dll")[0] + ".exe";
                    Process.Start(path);
                    Environment.Exit(0);

                    //return Task.CompletedTask;
                }
                else
                {
                    // ReSharper disable once AssignNullToNotNullAttribute

                    if (message.Exception.Message == null)
                    {
                        try
                        {
                            await Extensions.LoggerToChannel(message.Message, severity);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Ошибка!!!!!!! {e.Message}\n{e.StackTrace}");
                        }
                    }
                    else
                    {
                        try
                        {
                            await Extensions.LoggerToChannel(message.Message, severity,
                                $"#{message.Exception.Message} \n\n" +
                                $"{message.Exception.StackTrace}");

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Ошибка!!!!!!! {e.Message}\n{e.StackTrace}");
                        }
                    }
                }
            });
            
            return Task.CompletedTask;
        }
        
        static ServiceProvider ConfigureServices ( DiscordSocketConfig configuration, IConfiguration _configuration)
        {
            return new ServiceCollection()
                .AddSingleton(new DiscordSocketClient(configuration))
                //.AddSingleton<DiscordSocketClient>()
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton(x => new CommandService())
                .AddSingleton<AllEventsHadler>()
                .AddSingleton<SlashCommandHandler>()
                .AddSingleton<ClientHandler>()
                .AddSingleton<SnowesHandler>()
                .AddSingleton<PaintHandler>()
                .AddSingleton<UserCommands>()
                .AddSingleton<AdminCommands>()
                .AddSingleton<Users>()
                .AddSingleton<Castles>()
                .AddSingleton<BlockChannels>()
                .AddSingleton<InteractiveService>()

                .AddDbContextFactory<DiscordBotDbContext>(options => 
                    options.UseMySql(_configuration.GetConnectionString("Default"),
                        new MySqlServerVersion(new Version(8,0,28)), 
                        builder => builder.EnableRetryOnFailure(
                            maxRetryCount:5,
                            maxRetryDelay: System.TimeSpan.FromSeconds(30), 
                            errorNumbersToAdd: null)))
                



                .BuildServiceProvider();
        }
    }
}