using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using Infrastructure.DataAccessLayer;
using Microsoft.Extensions.Configuration;
using NewYearBot_Funclub.Utilites;
using Serilog;
using Serilog.Events;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace NewYearBot_Funclub.Services
{
    public class PaintHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly Users _users;
        public static readonly List<List<PaintBLock>> PaintLists = new();
        private InteractiveService Interactive { get; }

        public PaintHandler(DiscordSocketClient client,  InteractiveService interactive, Users users)
        {
            _client = client;
            Interactive = interactive;
            _users = users;
        }
        
        public async Task InitializeAsync ( )
        {
            // Add the public modules that inherit InteractionModuleBase<T> to the InteractionService

            // Process the InteractionCreated payloads to execute Interactions commands

            // Process the command execution results 
        }
        
        private async Task SlashCommandExecuted (SlashCommandInfo arg1, IInteractionContext arg2, IResult arg3)
        {
            Log.Information("Command: {arg1} ({arg2})", arg2.Interaction.Id, arg1.Name);
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
            if (command.Type == InteractionType.MessageComponent)
            {
                var t = ((SocketMessageComponent)command);
                var config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: true)
                    .Build();
                if (t.Message.Id.ToString() != config["MapMessageId"])
                {
                    return;
                }
                await command.DeferAsync();
                var message = t.Message.Content;
                string text = ":zero::one::two::three::four::five::six::seven::eight::nine::keycap_ten:\n";
                if (PaintLists.Count == 0)
                {
                    var newMsg = message.Split("\n");
                    for (var index = 1; index < newMsg.Length; index++)
                    {
                        var nm = newMsg[index];
                        var nmT = nm.Remove(0, 22);
                        var br = new List<PaintBLock>();
                        foreach (var vvv in nmT)
                        {
                            var c = new PaintBLock($"{vvv}");
                            br.Add(c);
                        }
                        PaintLists.Add(br);
                    }
                    
                }

                var snowers = await _users.GetSnowball(command.User.Id);
                if (snowers < 1)
                {
                    var embed2 = await MessageHelper.CreateEmbedAsync(command.User, _client, "Раскраска невозможна",
                        $"У тебя не хватает снежинок чтобы провести закраску");
                    await command.FollowupAsync(embed: embed2, ephemeral: true);
                    return;
                }
                
                var emojiSquared = t.Data.CustomId switch
                {
                    "Black" => PaintEmoji.Black.ToString(),
                    "White" => PaintEmoji.White.ToString(),
                    "Blue" => PaintEmoji.Blue.ToString(),
                    "Brown" => PaintEmoji.Brown.ToString(),
                    "Green" => PaintEmoji.Green.ToString(),
                    "Orange" => PaintEmoji.Orange.ToString(),
                    "Purple" => PaintEmoji.Purple.ToString(),
                    "Red" => PaintEmoji.Red.ToString(),
                    "Yellow" => PaintEmoji.Yellow.ToString(),
                    _ => PaintEmoji.Black
                };

                var embed = await MessageHelper.CreateEmbedAsync(command.User, _client, "Начинаем раскраску!",
                    $"・Цвет: {emojiSquared}\n " +
                    $"Теперь выбирай первую координату");
                var firstButtons = new ComponentBuilder()
                    .WithButton(null, "a", ButtonStyle.Primary, Emoji.Parse(":regional_indicator_a:"))
                    .WithButton(null, "b", ButtonStyle.Primary, Emoji.Parse(":regional_indicator_b:"))
                    .WithButton(null, "c", ButtonStyle.Primary, Emoji.Parse(":regional_indicator_c:"))
                    .WithButton(null, "d", ButtonStyle.Primary, Emoji.Parse(":regional_indicator_d:"))
                    .WithButton(null, "e", ButtonStyle.Primary, Emoji.Parse(":regional_indicator_e:"))
                    .WithButton(null, "f", ButtonStyle.Primary, Emoji.Parse(":regional_indicator_f:"))
                    .WithButton(null, "g", ButtonStyle.Primary, Emoji.Parse(":regional_indicator_g:"))
                    .WithButton(null, "h", ButtonStyle.Primary, Emoji.Parse(":regional_indicator_h:"))
                    .WithButton(null, "i", ButtonStyle.Primary, Emoji.Parse(":regional_indicator_i:"))
                    .WithButton(null, "j", ButtonStyle.Primary, Emoji.Parse(":regional_indicator_j:"))
                    .Build();
                
                var msg = await command.FollowupAsync(embed: embed, components: firstButtons, ephemeral: true);
                
                var nts = await Interactive.NextMessageComponentAsync(x =>
                    x.Message.Id == msg.Id && x.User.Id == command.User.Id, timeout: TimeSpan.FromMinutes(10));
                if (nts.IsSuccess)
                {
                    var firstIndex = 0;
                    //await nts.Value.UpdateAsync(x => x.Components = new ComponentBuilder().Build());
                    firstIndex = nts.Value.Data.CustomId switch
                    {
                        "a" => 0,
                        "b" => 1,
                        "c" => 2,
                        "d" => 3,
                        "e" => 4,
                        "f" => 5,
                        "g" => 6,
                        "h" => 7,
                        "i" => 8,
                        "j" => 9,
                        _ => firstIndex
                    };
                    
                    embed = await MessageHelper.CreateEmbedAsync(command.User, _client, "Заканчиваем раскраску!",
                        $"・Цвет: {emojiSquared}\n" +
                        $"・Первая координата: {nts.Value.Data.CustomId.ToUpper()}\n" +
                        $"Теперь выбирай вторую координату");
                    var partButtons = new ComponentBuilder()
                        .WithButton(null, "one", ButtonStyle.Primary, Emoji.Parse(":one:"))
                        .WithButton(null, "two", ButtonStyle.Primary, Emoji.Parse(":two:"))
                        .WithButton(null, "three", ButtonStyle.Primary, Emoji.Parse(":three:"))
                        .WithButton(null, "four", ButtonStyle.Primary, Emoji.Parse(":four:"))
                        .WithButton(null, "five", ButtonStyle.Primary, Emoji.Parse(":five:"))
                        .WithButton(null, "six", ButtonStyle.Primary, Emoji.Parse(":six:"))
                        .WithButton(null, "seven", ButtonStyle.Primary, Emoji.Parse(":seven:"))
                        .WithButton(null, "eight", ButtonStyle.Primary, Emoji.Parse(":eight:"))
                        .WithButton(null, "nine", ButtonStyle.Primary, Emoji.Parse(":nine:"))
                        .WithButton(null, "ten", ButtonStyle.Primary, Emoji.Parse(":keycap_ten:"))
                        .Build();

                    await nts.Value.UpdateAsync(x =>
                    {
                        x.Embed = embed;
                        x.Components = partButtons;
                    });
                    
                    
                    nts = await Interactive.NextMessageComponentAsync(x =>
                        x.Message.Id == msg.Id && x.User.Id == command.User.Id, timeout: TimeSpan.FromMinutes(10));
                    if (nts.IsSuccess)
                    {
                        var partIndex = 0;
                        //await nts.Value.UpdateAsync(x => x.Components = new ComponentBuilder().Build());
                        partIndex = nts.Value.Data.CustomId switch
                        {
                            "one" => 0,
                            "two" => 1,
                            "three" => 2,
                            "four" => 3,
                            "five" => 4,
                            "six" => 5,
                            "seven" => 6,
                            "eight" => 7,
                            "nine" => 8,
                            "ten" => 9,
                            _ => partIndex
                        };

                        PaintLists[firstIndex][partIndex].BlcokEmoji = t.Data.CustomId switch
                        {
                            "Black" => PaintEmoji.Black.ToString(),
                            "White" => PaintEmoji.White.ToString(),
                            "Blue" => PaintEmoji.Blue.ToString(),
                            "Brown" => PaintEmoji.Brown.ToString(),
                            "Green" => PaintEmoji.Green.ToString(),
                            "Orange" => PaintEmoji.Orange.ToString(),
                            "Purple" => PaintEmoji.Purple.ToString(),
                            "Red" => PaintEmoji.Red.ToString(),
                            "Yellow" => PaintEmoji.Yellow.ToString(),
                            _ => PaintLists[firstIndex][partIndex].BlcokEmoji
                        };

                        await msg.DeleteAsync();
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
                
                
                
                var i = 0;
                foreach (var br in PaintLists)
                {
                    switch (i)
                    {
                        case 0:
                            text += ":regional_indicator_a:";
                            break;
                        case 1:
                            text += ":regional_indicator_b:";
                            break;
                        case 2:
                            text += ":regional_indicator_c:";
                            break;
                        case 3:
                            text += ":regional_indicator_d:";
                            break;
                        case 4:
                            text += ":regional_indicator_e:";
                            break;
                        case 5:
                            text += ":regional_indicator_f:";
                            break;
                        case 6:
                            text += ":regional_indicator_g:";
                            break;
                        case 7:
                            text += ":regional_indicator_h:";
                            break;
                        case 8:
                            text += ":regional_indicator_i:";
                            break;
                        case 9:
                            text += ":regional_indicator_j:";
                            break;
                    }

                    i++;
                    foreach (var b in br)
                    {
                        text += b.BlcokEmoji.ToString() + "";
                    }
                    text += "\n";
                }

                await _users.ModifySnowball(command.User.Id, -1); 
                await command.ModifyOriginalResponseAsync(x => x.Content = text);
                await Program.MapLogger.SendMessageAsync(text);
            }
        }
        
    }
}