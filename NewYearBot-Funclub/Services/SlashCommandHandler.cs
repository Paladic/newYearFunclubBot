using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using NewYearBot_Funclub.Utilites;
using Serilog;
using Serilog.Events;
using IResult = Discord.Interactions.IResult;
// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace NewYearBot_Funclub.Services
{
    public class SlashCommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _commands;
        private readonly IServiceProvider _services;

        public SlashCommandHandler(DiscordSocketClient client, InteractionService commands, 
            IServiceProvider services)
        {
            _client = client;
            _commands = commands;
            _services = services;
        }

        public async Task InitializeAsync ( )
        {
            // Add the public modules that inherit InteractionModuleBase<T> to the InteractionService
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            // Process the InteractionCreated payloads to execute Interactions commands

            // Process the command execution results 
            _commands.SlashCommandExecuted += SlashCommandExecuted;
        }
        
        private async Task SlashCommandExecuted (SlashCommandInfo arg1, IInteractionContext arg2, IResult arg3)
        {
            // await arg2.Interaction.DeferAsync(false);
            string ermsg = "";
            if (!arg3.IsSuccess)
            {
                string error = "";
                switch (arg3.ErrorReason)
                {
                    case "The input text has too few parameters.":
                        error = "Вами не были указаны все параметры, проверьте и попробуйте еще раз.";
                        break;
                    case "The input text has too many parameters.":
                        error = "Вы указали слишком много параметров, проверьте и попробуйте еще раз.";
                        break;
                    case "Unknown command.":
                        error = "Неизвестная команда.";
                        break;
                    case "User not found.":
                        error = "Пользователь не найден.";
                        break;
                    case "User requires guild permission Administrator.":
                        error = "Для использования команды тебе необходимы права администратора.";
                        break;
                    case "Bot requires guild permission ManageRoles.":
                        error = "Для использования этой команды мне необходимы права на работу с ролями.";
                        break;
                    case "Command precondition group Администратор failed.":
                        error = "Для использования команды тебе необходимы права администратора.";
                        break;
                    case "Module precondition group Администратор failed.":
                        error = "Для использования команды тебе необходимы права администратора.";
                        break;
                    case "Command precondition group Модерация failed.":
                        error = "Для использования команды тебе необходимы права \"Отправить пользователя думать над своим поведением\"";
                        break;
                    case "The server responded with error 10008: Unknown Message":
                        return;
                    case "Cannot respond to an interaction after 3 seconds!":
                        error = "Ууппс... Извини, сейчас я испытываю сильную нагрузку или хост перегружен. Подожди немного" +
                                "\n- *Если это сообщение появляется уже очень давно, то сообщите об этом ответственному лицу*";
                        break;
                    case "Cannot defer an interaction after 3 seconds!":
                        error = "Ууппс... Подожди немного (использую команду снова через 15-20 секунд)" +
                                "\n- *Если это сообщение появляется уже очень давно, то сообщите об этом ответственному лицу*";
                        break;
                    default:
                        error = $"```\n{arg3.ErrorReason}```\n● Необработанная ошибка. Сообщите об этом ответственному лицу `";

                        break;
                }

                Embed embed = await MessageHelper.CreateErrorEmbedAsync(error);
                switch (arg3.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        embed = await  MessageHelper.CreateErrorEmbedAsync(error);
                        //await arg2.Interaction.RespondAsync(embed: embed);
                        break;
                    case InteractionCommandError.UnknownCommand:
                        embed = await  MessageHelper.CreateErrorEmbedAsync(error);
                        //await arg2.Interaction.RespondAsync(embed: embed);
                        break;
                    case InteractionCommandError.BadArgs:
                        embed = await  MessageHelper.CreateErrorEmbedAsync(error);
                        //await arg2.Interaction.RespondAsync(embed: embed);
                        break;
                    case InteractionCommandError.Exception:
                        embed = await  MessageHelper.CreateErrorEmbedAsync(error);
                        //await arg2.Interaction.RespondAsync(embed: embed);
                        break;
                    case InteractionCommandError.Unsuccessful:
                        embed = await  MessageHelper.CreateErrorEmbedAsync(error);
                        //await arg2.Interaction.RespondAsync(embed: embed);
                        break;
                }

                try
                {
                    await arg2.Interaction.RespondAsync(embed: embed);
                }
                catch (Exception)
                {
                    try
                    {
                        await arg2.Interaction.ModifyOriginalResponseAsync(x => x.Embed = embed);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            await arg2.Channel.SendMessageAsync(embed: embed);
                        }
                        catch
                        {
                            //
                        }
                    }
                }
                ermsg = $". Команда выдала ошибку: {arg3.ErrorReason}";
            }
            
            if(arg2.Guild != null)
            {
                var log = $"{arg2.User.Username}#{arg2.User.Discriminator}({arg2.User.Id}) использовал \"{arg1.Name}\" " +
                          $"в #{arg2.Channel.Name}({arg2.Channel.Id}) {ermsg}";

                Log.Information("{Username}#{Discriminator}({UserId}) использовал \"{CommandName}\" " +
                                "в #{ChannelName}({ChannelId}) {Error}", arg2.User.Username, arg2.User.Discriminator, arg2.User.Id,
                    arg1.Name, arg2.Channel.Name, arg2.Channel.Id, ermsg);
                await Extensions.LoggerToChannel(log, LogEventLevel.Information);
                
                // return Task.CompletedTask;
            }
        }

        public async Task HandleInteraction (SocketInteraction command)
        {
            if (command.Type != InteractionType.ApplicationCommand)
            {
                return;
            }
            /*  if (command.Channel.GetType().ToString() == "Discord.WebSocket.SocketDMChannel")
              {
                  await command.Channel.SendMessageAsync("Прости, но, тут я не работаю.");
                  return;
              }*/
            try
            {
                await command.DeferAsync();
                
                // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules
                var ctx = new SocketInteractionContext(_client, command);
                await _commands.ExecuteCommandAsync(ctx, _services);
            }
            catch (Exception)
            {
                // If a Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
                // response, or at least let the user know that something went wrong during the command execution.
                if(command.Type == InteractionType.ApplicationCommand)
                    await command.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }
    }
}