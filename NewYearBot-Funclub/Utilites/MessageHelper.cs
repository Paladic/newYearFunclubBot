using Discord;

namespace NewYearBot_Funclub.Utilites;

public static class MessageHelper
{
    
    public static async Task<Embed> CreateEmbedAsync(IUser user,
        IDiscordClient client, string title, string description, string url = null, string footerPhrase = null, Color? color = null, 
        string thumbUrl = null, EmbedFieldBuilder[] fields = null)
    {
        var embed = new EmbedBuilder()
            .WithAuthor(await CreateAuthorEmbed(user))
            .WithThumbnailUrl(thumbUrl)
            .WithColor(color ?? new Color(0, 209, 202))
            .WithTitle(title)
            .WithDescription(description)
            .WithImageUrl(url);
            //.WithFooter(await CreateFooterEmbed((DiscordSocketClient)client, footerPhrase));
        
        if (fields != null)
        {
            embed.WithFields(fields);
        }
            
        return embed.Build();
    }
    
    public static Task<Embed> CreateErrorEmbedAsync(string description)
    {
        var embed = new EmbedBuilder()
            .WithColor(new Color(255, 100, 100))
            .WithDescription(description)
            .WithAuthor(author =>
            {
                author
                    .WithIconUrl(
                        "https://cdn.discordapp.com/attachments/890682513503162420/1052914096955195452/pngwing.com.png")
                    .WithName("Произошла ошибка:");
            })
            .Build();
            
        return Task.FromResult(embed);
    }
    public static Task<EmbedAuthorBuilder> CreateAuthorEmbed(IUser user)
    {
        
        var authorBuilder = new EmbedAuthorBuilder()
            .WithName(user.Username + "#" + user.Discriminator)
            .WithIconUrl(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl());

        return Task.FromResult(authorBuilder);
    }
    
    public static Task<EmbedFooterBuilder> CreateFooterEmbed(IDiscordClient client, string footerPharse = null)
    {
        var eleinaFooterBuilder = new EmbedFooterBuilder()
            .WithText(footerPharse)
            .WithIconUrl(client.CurrentUser.GetAvatarUrl());

        return Task.FromResult(eleinaFooterBuilder);
        // await Extensions.createFooterEmbed(Context.Client) 
    }
    

}