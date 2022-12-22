using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Fergun.Interactive;
using Infrastructure.DataAccessLayer;
using NewYearBot_Funclub.Services;
using NewYearBot_Funclub.Utilites;
using Serilog;

namespace NewYearBot_Funclub.Commands;

public class AdminCommands : InteractionModuleBase<SocketInteractionContext>
{
    private readonly Users _users;
    private readonly Castles _castles;
    private readonly BlockChannels _blockChannels;
    private InteractiveService Interactive { get; }
        
    public AdminCommands(Users users, Castles castles, InteractiveService interactive, BlockChannels blockChannels)
    {
        _users = users;
        _castles = castles;
        Interactive = interactive;
        _blockChannels = blockChannels;
    }

    [SlashCommand("оп-сказать", "отправить сообщение в нужный канал", runMode: RunMode.Async)]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [EnabledInDm(false)]
    public async Task ChooseYourTeam([Summary("канал", "канал куда будем писать")]SocketTextChannel channel, 
        [Summary("текст", "текст сообщения")]string text)
    {
        var embed = await MessageHelper.CreateEmbedAsync(Context.User, Context.Client, "Вниамние! Внимание!", text);
        try
        {
            await channel.SendMessageAsync(embed: embed);
        }
        catch
        {
            await Context.Interaction.ModifyOriginalResponseAsync(x =>
                x.Content = "у меня нет прав туда писать, ну ты и мда");
            return;
        }
        
        await Context.Interaction.ModifyOriginalResponseAsync(x => x.Content = $"отправлено");
    }

    [SlashCommand("оп-выдать-снежки", "выдает пользователю указанное количество снежков", runMode: RunMode.Async)]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [EnabledInDm(false)]
    public async Task GiveUserSnowballs(
        [Summary("пользователь", "кому будем давать (можно и отрицательно)")] SocketUser user,
        [Summary("сумма", "сколько даем")] int count)
    {
        Embed embed;
        var userS = await _users.GetUser(user.Id);
        if (userS == null)
        {
            embed = await MessageHelper.CreateEmbedAsync(Context.User, Context.Client, "Эй!",
                "Такого пользователя пока что нет");
            await Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = embed);
            return;
        }

        await _users.ModifySnowball(user.Id, count);
        var userS2 = await _users.GetUser(user.Id);

        embed = await MessageHelper.CreateEmbedAsync(Context.User, Context.Client, "Готово!",
            $"Теперь у {user.Mention} `{userS2.Snowball}` снежков (было `{userS.Snowball}` снежков)");
        await Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = embed);

    }

    [SlashCommand("оп-создать-замки", "создает замки, что, логично", runMode: RunMode.Async)]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [EnabledInDm(false)]
    public async Task CreateCastles([Summary("роль-красных", "роль для красных")] SocketRole redRole,
        [Summary("роль-синих", "роль для красных")] SocketRole blueRole,
        [Summary("роль-зеленых", "роль для красных")] SocketRole greenRole)
    {
        Embed embed;
        var castle = await _castles.GetCastles();
        if (castle.Count > 0)
        {
            embed = await MessageHelper.CreateEmbedAsync(Context.User, Context.Client, "ТИХО ТИХО ТИХО!",
                "Эту команду можно использовать " +
                "**ТОЛЬКО ОДИН РАЗ**. Серьезно, если сделали что-то не так, то необходимо полностью пересоздавать Базу данных, так как это привязано к ID!!");
            await Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = embed);
            return;
        }

        await _castles.NewCastle("Красная", redRole.Id);
        await _castles.NewCastle("Синяя", blueRole.Id);

        embed = await MessageHelper.CreateEmbedAsync(Context.User, Context.Client, "Готово!",
            "Мы готовы к запуску, вроде бы");
        await Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = embed);

    }
    
    /*
     *
     *
     *
     *
     * 
     */

    [SlashCommand("оп-полотно-вызвать", "создает полотно для рисования", runMode: RunMode.Async)]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [EnabledInDm(false)]
    public async Task CreatePaint()
    {
        int n = 10;
        int m = 10;
        PaintHandler.PaintLists.Clear();

        var count2 = 1;
        string text = ":zero::one::two::three::four::five::six::seven::eight::nine::keycap_ten:\n";
        for (var i = 0; i < n; i++)
        {
            var br = new List<PaintBLock>();
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
            for (var j = 0; j < m; j++)
            {
                
                text += PaintEmoji.White;
                var c = new PaintBLock(PaintEmoji.White.ToString());
                br.Add(c);
            }
            
            PaintHandler.PaintLists.Add(br);

            text += "\n";
        }

        var buttons = new ComponentBuilder()
            .WithButton(null, "Black", ButtonStyle.Primary, PaintEmoji.Black)
            .WithButton(null, "White", ButtonStyle.Primary, PaintEmoji.White)
            .WithButton(null, "Blue", ButtonStyle.Primary, PaintEmoji.Blue)
            .WithButton(null, "Brown", ButtonStyle.Primary, PaintEmoji.Brown)
            .WithButton(null, "Green", ButtonStyle.Primary, PaintEmoji.Green)
            .WithButton(null, "Orange", ButtonStyle.Primary, PaintEmoji.Orange)
            .WithButton(null, "Purple", ButtonStyle.Primary, PaintEmoji.Purple)
            .WithButton(null, "Red", ButtonStyle.Primary, PaintEmoji.Red)
            .WithButton(null, "Yellow", ButtonStyle.Primary, PaintEmoji.Yellow)
            .Build();

        
        var msg = await Context.Interaction.ModifyOriginalResponseAsync(x =>
        {
            x.Content = text;
            x.Components = buttons;
        });
        
        string json = File.ReadAllText("appsettings.json");
        dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
        jsonObj["MapMessageId"] = msg.Id;
        string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText("appsettings.json", output);
        
    }

    [SlashCommand("оп-канал-настройка", "настраиваете канал на снежки", runMode: RunMode.Async)]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [EnabledInDm(false)]
    public async Task DisableCommands([Summary("канал", "с каким каналом работаем")]SocketGuildChannel channel, 
        [Choice("отключить-снежки", "disable"), Choice("включить-снежки", "enabled"), 
         Summary("переключатель", "включаем или выключаем снежки в канале")]string pereck)
    {
        if (pereck == "disable")
        {
            await _blockChannels.NewChannel(channel.Id);
            var embed1 = await MessageHelper.CreateEmbedAsync(Context.User, Context.Client, "Канал отключен",
                "теперь в канале не будут даваться снежки");
            await Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = embed1);
        }
        else
        {
            var embed2 = await MessageHelper.CreateEmbedAsync(Context.User, Context.Client, "Канал включен",
                "теперь в канале будут даваться снежки");
            await _blockChannels.DeleteChannel(channel.Id);
            await Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = embed2);
        }
    }
    
}