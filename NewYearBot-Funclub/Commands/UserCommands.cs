using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using Infrastructure.DataAccessLayer;
using Infrastructure.Models;
using NewYearBot_Funclub.Utilites;

namespace NewYearBot_Funclub.Commands;

public class UserCommands : InteractionModuleBase<SocketInteractionContext>
{
    private readonly Users _users;
    private readonly Castles _castles;
    private readonly Random rnd = new Random();
    private InteractiveService Interactive { get; }
    
    public UserCommands(Users users, Castles castles, InteractiveService interactive)
    {
        _users = users;
        _castles = castles;
        Interactive = interactive;
    }

    [SlashCommand("выбрать-команду", "Выбор команды, за которую будешь сражаться", runMode: RunMode.Async)]
    [DefaultMemberPermissions(GuildPermission.ViewChannel)]
    [EnabledInDm(false)]
    public async Task ChooseYourTeam([Choice("Красная", 1),
                                            Choice("Синяя", 2),
                                            Choice("Зелёная", 3),
                                           Summary("Команда", "за кого сражаемся?")]
        ulong team)
    {
        var userTeam = await _users.GetCastleId(Context.User.Id);
        Embed embed;
        if (userTeam != 0)
        {
            embed = await MessageHelper.CreateEmbedAsync(Context.User, Context.Client, "Ты куда собрался, дизертир",
                "Извини, но команду сменить нельзя!");
            await Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = embed);
            return;
        }

        var castles = await _castles.GetCastles();
        var castle = await _castles.GetCastleFromId(team);
        int castleCount = 0;
        foreach (var castleR in castles)
        {
            castleCount += Context.Guild.GetRole(castleR.RoleId).Members.Count();
        }

        if (castleCount / 3 < Context.Guild.GetRole(castle.RoleId).Members.Count() &&
            Context.Guild.GetRole(castle.RoleId).Members.Count() > 5)
        {
            embed = await MessageHelper.CreateEmbedAsync(Context.User, Context.Client,
                "Уупс, в этой крепости пока нет места для тебя!", "Слушай, за эту сторону и так все воют, выбери другую, хорошо?");
            await Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = embed);
            return;
        }

        await _users.ModifyCastleId(Context.User.Id, team);
        try
        {
            await ((SocketGuildUser)Context.User).AddRoleAsync(castle.RoleId);
        }
        catch
        {
            // похуй
        }
        embed = await MessageHelper.CreateEmbedAsync(Context.User, Context.Client, "Команда успешно выбрана!",
            $"Поздравляю, теперь ты сражаешься за сторону: `{castle.Name}`");
        await Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = embed);
    }
    
    [SlashCommand("атаковать", "кинуть снежок в крепость другой команды", runMode: RunMode.Async)]
    [DefaultMemberPermissions(GuildPermission.ViewChannel)]
    [EnabledInDm(false)]
    public async Task AttackEnemyTeamAsync([Choice("Красная", 1), 
                                            Choice("Синяя", 2), 
                                            Choice("Зелёная", 3), Summary("Команда", "кого будет атковать")]ulong team)
    {
        var userTeam = await _users.GetCastleId(Context.User.Id);
        Embed embed;
        if (userTeam == 0)
        {
            embed = await MessageHelper.CreateEmbedAsync(Context.User, Context.Client, "Нет-нет",
                "Для начала тебе стоит выбрать команду, попробуй написать команду `/выбрать-команду`");
            await Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = embed);
            return;
        }

        if (userTeam == team)
        {
            embed = await MessageHelper.CreateEmbedAsync(Context.User, Context.Client, "Ты куда воюешь?",
                "Ты не в Among Us, чтобы становиться предателем, окей?");
            await Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = embed);
            return;
        }

        if (await _users.GetDamageEnd(Context.User.Id) > (ulong)DateTimeOffset.Now.ToUnixTimeSeconds())
        {
            embed = await MessageHelper.CreateEmbedAsync(Context.User, Context.Client, "Стряхни с себя снег для начала",
                $"Твое время для сражения еще не подошло к концу, приходи <t:{await _users.GetDamageEnd(Context.User.Id)}:R>");
            await Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = embed);
            return;
        }

        var castle = await _castles.GetCastleFromId(team);
        await _users.ModifySnowball(Context.User.Id, -1);

        if (castle.CastleSize > 0)
        {
            var chance = rnd.Next(0, 100);
            if (chance >= (30 + castle.SnowmanCount * 5))
            {
                await _castles.ModifyCastleSize(team, -1);
                embed = await MessageHelper.CreateEmbedAsync(Context.User, Context.Client, "Попадание!",
                    "Нам удалось поразить вражескую крепость!");
                await Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = embed);
            }
            else
            {
                embed = await MessageHelper.CreateEmbedAsync(Context.User, Context.Client, "Мимо!",
                    "Ну куда ты стреляешь! Ты промазал.");
                await Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = embed);
            }
        }
        else
        {
            var chance = rnd.Next(0, 100);
            if (chance >= 30)
            {
                // успех
                if (castle.SnowmanCount > 0)
                {
                    await _castles.ModifySnowmanCount(team, -1);
                    embed = await MessageHelper.CreateEmbedAsync(Context.User, Context.Client, "Попадание!",
                        "Мы попали, но они спаслись с помощью снеговика!");
                    await Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = embed);
                }
                else
                {
                    var teamList = await _users.GetTeamList(castle.Id);
                    if (teamList.Count == 0)
                    {
                        embed = await MessageHelper.CreateEmbedAsync(Context.User, Context.Client, "Уупс...",
                            "Видимо, никого из этой команды не осталось, как-то неловко даже");
                        await Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = embed);
                        return;
                    }
                    var user = teamList[rnd.Next(0, teamList.Count)];
                    await _users.ModifyDamageEnd(user.Id, (ulong) (DateTimeOffset.Now + TimeSpan.FromHours(1)).ToUnixTimeSeconds());
                    embed = await MessageHelper.CreateEmbedAsync(Context.User, Context.Client, "Успешное ранение!",
                        $"Вот так вот! Мы вевели из строя <@!{user.Id}> на 1 час, ай да молодцы!");
                    await Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = embed);
                    
                    embed = await MessageHelper.CreateEmbedAsync(Context.User, Context.Client, "Уупс! В тебя попали!",
                        $"Привет! Представляешь, в тебя прилетел снежок от {Context.User.Mention} и теперь ты не сможешь сражаться до " +
                        $"<t:{(DateTimeOffset.Now + TimeSpan.FromHours(1)).ToUnixTimeSeconds()}:f>");

                    var userR = Context.Guild.GetUser(user.Id);
                    try
                    {
                        
                        await userR.SendMessageAsync(embed: embed);
                    }
                    catch
                    {
                        // ошибка!
                    }
                    
                }
            }
            else
            {
                embed = await MessageHelper.CreateEmbedAsync(Context.User, Context.Client, "Мимо!",
                    "Ну куда ты стреляешь! Ты промазал.");
                await Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = embed);
            }
           
        }

    }

    [SlashCommand("профиль", "просмотреть свой или чужой профиль", runMode: RunMode.Async)]
    [DefaultMemberPermissions(GuildPermission.ViewChannel)]
    [EnabledInDm(false)]
    public async Task ProfileAsync(
        [Summary("пользователь", "дополнение, если будем чей-то профиль смотреть")]SocketGuildUser? user = null)
    {
        user ??= (SocketGuildUser) Context.User;

        var userStats = await _users.GetUser(user.Id);
        var castle = await _castles.GetCastleFromId(userStats.CastleId);
        var team = new EmbedFieldBuilder()
            .WithName("Команда")
            .WithValue("Команда не выбрана")
            .WithIsInline(true);
            
        if (castle != null)
        {
            team = new EmbedFieldBuilder()
                .WithName("Команда")
                .WithValue($"{castle.Name}")
                .WithIsInline(true);
        }

        var s = new EmbedFieldBuilder()
            .WithName("Ранения")
            .WithValue($"Полностью живой");
        if (userStats.DamageEnd > (ulong) DateTimeOffset.Now.ToUnixTimeSeconds())
        {
            s = new EmbedFieldBuilder()
                .WithName("Ранения")
                .WithValue($"истекает <t:{userStats.DamageEnd}:R>");
        }
        
        var snowball = new EmbedFieldBuilder()
            .WithName("Снежки")
            .WithValue($"{userStats.Snowball} шт.")
            .WithIsInline(true);

        var embed = await MessageHelper.CreateEmbedAsync(Context.User, Context.Client,
            $"Профиль: {user.Username}#{user.Discriminator}",
            "", url: "https://cdn.discordapp.com/attachments/890682513503162420/1053339674472611920/footprints-in-the-snow.png" , 
            fields: new []{team, snowball, s});

        await Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = embed);
    }

    [SlashCommand("замок-инфо", "просмотреть на чужой замок", runMode: RunMode.Async)]
    [DefaultMemberPermissions(GuildPermission.ViewChannel)]
    [EnabledInDm(false)]
    public async Task ProfileCastleAsync([Choice("Красная", 1), 
                                          Choice("Синяя", 2), 
                                          Choice("Зелёная", 3), Summary("Команда", "кого будет атковать")]ulong team)
    {
        var endDate = DateTimeOffset.Now + TimeSpan.FromMinutes(10);

        while (endDate > DateTimeOffset.Now)
        {

            var userTeam = await _users.GetUser(Context.User.Id);
            var castleInfo = await _castles.GetCastleFromId(team);
            var castleSize = new EmbedFieldBuilder()
                .WithName("Размер крепости")
                .WithValue($"{castleInfo.CastleSize / 10} комов | {castleInfo.CastleSize % 10} комочков")
                .WithIsInline(true);

            var snowmans = new EmbedFieldBuilder()
                .WithName("Снеговики")
                .WithValue($"{castleInfo.SnowmanCount} шт")
                .WithIsInline(true);

            var roleCount = Context.Guild.GetRole(castleInfo.RoleId).Members.Count();
            var counters = new EmbedFieldBuilder()
                .WithName("Участники")
                .WithValue($"{roleCount} <@&{castleInfo.RoleId}>");

            
            var castleSizeDisable = userTeam == null || (userTeam.CastleId != castleInfo.Id || userTeam.Snowball < 10 || userTeam.DamageEnd >  (ulong) DateTimeOffset.Now.ToUnixTimeSeconds());
            var snowballCountDisable = userTeam == null || (castleInfo.SnowmanCount >= 10 || userTeam.CastleId != castleInfo.Id || userTeam.Snowball < 15 || userTeam.DamageEnd >  (ulong) DateTimeOffset.Now.ToUnixTimeSeconds());

            var buttons = new ComponentBuilder()
                .WithButton("Слепить ком (10 снежков)", "add-castlesize", ButtonStyle.Success,
                    disabled: castleSizeDisable)
                .WithButton("Слепить снеговиков (15 снежков)", "add-snowman", ButtonStyle.Success,
                    disabled: snowballCountDisable)
                .Build();

            var embed = await MessageHelper.CreateEmbedAsync(Context.User, Context.Client,
                $"Профиль команды {castleInfo.Name}", "",
                url:
                "https://cdn.discordapp.com/attachments/890682513503162420/1053343485257850910/snowcastle-of-kemi-entrance.png",
                fields: new[] { castleSize, snowmans, counters });

            var msg = await Context.Interaction.ModifyOriginalResponseAsync(x =>
            {
                x.Embed = embed;
                x.Components = buttons;
            });
            
            var nts = await Interactive.NextMessageComponentAsync(x =>
                x.Message.Id == msg.Id && x.User.Id == Context.User.Id, timeout: TimeSpan.FromMinutes(10));
            if (nts.IsSuccess)
            {
                await nts.Value.UpdateAsync(x => x.Components = new ComponentBuilder().Build());
                switch (nts.Value.Data.CustomId)
                {
                    case "add-castlesize":
                        await _users.ModifySnowball(Context.User.Id, -10);
                        await _castles.ModifyCastleSize(team, 1 * 10);
                        embed = await MessageHelper.CreateEmbedAsync(Context.User, Context.Client,
                            "Мы успешно слепили ком", $"Ура! Теперь у нашего замка {castleInfo.CastleSize / 10 + 1} комов.");
                        await Context.Interaction.FollowupAsync(embed: embed,  ephemeral: true);
                        break;
                    case "add-snowman":
                        await _users.ModifySnowball(Context.User.Id, -15);
                        await _castles.ModifySnowmanCount(team, 1);
                        embed = await MessageHelper.CreateEmbedAsync(Context.User, Context.Client,
                            "Мы успешно слепили снеговика", $"Ура! Теперь у нашего замка {castleInfo.SnowmanCount + 1} снеговиков.");
                        await Context.Interaction.FollowupAsync(embed: embed, ephemeral: true);
                        break;
                }
            }
                
            await Task.Delay(1 * 3 * 1000);
        }
        
        await Context.Interaction.ModifyOriginalResponseAsync(x => x.Components = new ComponentBuilder().Build());

    }

    [SlashCommand("бот-инфо", "просмотреть информацию о боте", runMode: RunMode.Async)]
    [DefaultMemberPermissions(GuildPermission.ViewChannel)]
    [EnabledInDm(false)]
    public async Task BotInfo()
    {
        var dev = await Context.Client.GetUserAsync(219535226462928896);
        var idea = await Context.Client.GetUserAsync(437540824524521472);
        var fieldInfo = new EmbedFieldBuilder()
            .WithName("Информация о боте")
            .WithValue($"・ Аптайм: <t:{Extensions.CurUpTime.ToUnixTimeSeconds()}:f>\n" +
                       $"・ Креатор: {idea.Mention} ({idea.Username}#{idea.Discriminator})\n" +
                       $"・ Разработчик: {dev.Mention} ({dev.Username}#{dev.Discriminator})");

        var embed = await MessageHelper.CreateEmbedAsync(Context.User, Context.Client, "Новогоднее веселье!", 
            "Новогодний бот с функционалом снежной битвы и пиксель-батлом. \n" +
            "・ Закрашивайте пиксели и рисуйте свои картины.\n" +
            "・ Общайтесь в текстом или голосом и зарабатывайте снежки, с помощью которых закидывайте вражеские крепости!\n" +
            "・ Уничтожайте врагов, обустраиваете крепости!\n" +
            "・ Для начала игры напишите `/выбрать-команду`", 
            //url: "https://media.discordapp.net/attachments/890682513503162420/1053792825092886608/onding-3320-f3966e1b069ae09269b14db2a4247d1a1x.png",
            thumbUrl: "https://media.discordapp.net/attachments/890682513503162420/1053792874187210883/snowflake-threads-wool-coat.png", 
            fields: new []{fieldInfo});

        var buttons = new ComponentBuilder()
            .WithButton("Ссылки на разработчика:", "asd1", ButtonStyle.Primary, disabled: true, row: 0)
            .WithButton("Boosty ", null, ButtonStyle.Link, Emoji.Parse("🎁"), url: "https://boosty.to/paladic", row: 0)
            .WithButton("Sbertips ", null, ButtonStyle.Link, Emoji.Parse("🪙"), url: "https://pay.mysbertips.ru/79338714", row: 0)
            .WithButton("Ссылки на креатора", "asd", ButtonStyle.Primary, disabled: true, row: 1)
            .WithButton("Sbertips ", null, ButtonStyle.Link, Emoji.Parse("🪙"), url: "https://pay.mysbertips.ru/50701604", row: 1)
            .Build();

        await Context.Interaction.ModifyOriginalResponseAsync(x =>
        {
            x.Embed = embed;
            x.Components = buttons;
        });
    }
    
}