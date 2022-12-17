using Discord;
using Discord.WebSocket;
using Infrastructure.DataAccessLayer;
using NewYearBot_Funclub.Utilites;

namespace NewYearBot_Funclub.Services;

public class SnowesHandler
{
    private readonly DiscordSocketClient _client;
    public static readonly List<SnowesStorage.TextSnowes> TextSnowes = new();
    public static readonly List<SnowesStorage.VoiceSnowes> VoiceSnowes = new();
    private readonly Users _users;
    private readonly Random Rand = new();


    public SnowesHandler(DiscordSocketClient client, Users users)
    {
        _client = client;
        _users = users;
    }
 
    public async Task XpVoiceSystem(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            if (user.IsBot) return;
            if (after.VoiceChannel != null)
            {
                if (VoiceSnowes.Count(x => x.User == user as SocketGuildUser) == 0)
                {
                    VoiceSnowes.Add(new SnowesStorage.VoiceSnowes() {User = user as SocketGuildUser, 
                        ConnectionStart = DateTime.Now, Mark = false,StoredSecond = 0});
                }
                
                var currentUser = VoiceSnowes
                    .FirstOrDefault(x => x.User == user as SocketGuildUser);

                var count = after.VoiceChannel.Users.Count(x => !x.IsBot && !x.IsDeafened && !x.IsSelfDeafened && !x.IsMuted && !x.IsSelfMuted && 
                                                                x.VoiceState != null && x.VoiceState.Value.VoiceChannel == after.VoiceChannel);

                if (currentUser == null) return;

                currentUser.CurrentVoice = after.VoiceChannel;

                foreach (var voiceLevel in VoiceSnowes
                             .Where(x => x.CurrentVoice == before.VoiceChannel || x.CurrentVoice == after.VoiceChannel))
                {

                    var userStatus = !voiceLevel.User.IsDeafened && !voiceLevel.User.IsMuted &&
                                     !voiceLevel.User.IsSelfDeafened && !voiceLevel.User.IsSelfMuted &&
                                     !voiceLevel.User.IsBot;
                    if (count > 1 && userStatus && !voiceLevel.Mark)
                    {
                        // var str = voiceLevel.StoredSecond;
                        voiceLevel.ConnectionStart = DateTime.Now;
                        voiceLevel.Mark = true;
                        // await (chd as ISocketMessageChannel).SendBaseEmbed(voiceLevel.User, Client, "Старт начисления активирован", $"Было{str}\nСтало: {voiceLevel.StoredSecond}"); // Дебагер, отселижвание когда юзер начал получать XP

                    }
                    else if ((count <= 1 || !userStatus) && voiceLevel.Mark)
                    {
                        // var str2 = voiceLevel.StoredSecond;
                        voiceLevel.StoredSecond += (DateTime.Now - voiceLevel.ConnectionStart).Seconds;
                        voiceLevel.Mark = false;
                        // await (chd as ISocketMessageChannel).SendBaseEmbed(voiceLevel.User, Client, "Старт начисления деактивирован", $"Было: {str2}\nСтало: {voiceLevel.StoredSecond}"); // Дебагер, отслеживание когда юзер перестал получать XP
                    }
                }
            }
            else
            {
                if(before.VoiceChannel == null) return;
                var currentUser = VoiceSnowes
                    .FirstOrDefault(x => x.User == user as SocketGuildUser);
                    
                if (currentUser != null)
                {
                    VoiceSnowes.Remove(currentUser);
                    var count = VoiceSnowes
                        .Count(x => x.User == user as SocketGuildUser);
                        
                    if (currentUser.StoredSecond == 0 && currentUser.Mark)
                    {
                        currentUser.StoredSecond += (DateTime.Now - currentUser.ConnectionStart).Seconds;
                    }
                    
                    //Log.Debug("{StoredSecond} || {Username} || {Count}", currentUser.StoredSecond, currentUser.User.Username, count);
                    // await (chd as ISocketMessageChannel).SendBaseEmbed(user, Client, "Проверка", $"{currentUser.StoredSecond}"); // Дебагер, проверка XP
                    var xpGrant = currentUser.StoredSecond;
                    await _users.ModifySnowball(currentUser.User.Id, xpGrant);

                }
            }
            
            /*
            * null -> voice 1 // зашел первый раз в войс
            * voice 1 -> voice 2 // зашел в другой войс 
            * voice 2 -> null // вышел из войса
            */
        }
        public async Task XpTextSystem(SocketMessage socketMessage)
        {
            if (socketMessage is not SocketUserMessage {Source: MessageSource.User} message) return;
            if (socketMessage.Author.IsBot || socketMessage.Author.IsWebhook) return;
            if (socketMessage.Channel.GetType().ToString() == "Discord.WebSocket.SocketDMChannel") return;
            
           
            if(!TextSnowes.Contains(new SnowesStorage.TextSnowes{User = message.Author as SocketGuildUser}))
                TextSnowes.Add(new SnowesStorage.TextSnowes{User = message.Author as SocketGuildUser, NewMessage = DateTime.Now});
            
            if (TextSnowes
                .Any(x => x.User == message.Author as SocketGuildUser))
            {
                var userL = TextSnowes
                    .FirstOrDefault(x => x.User == message.Author as SocketGuildUser);
                if (userL != null && userL.NewMessage > DateTime.Now) return;

                if (userL != null)
                {
                    var userId = userL.User.Id;
                    var xpGrant = Rand.Next(10, 20);
                    await _users.ModifySnowball(userId, xpGrant);
                
                    userL.NewMessage = DateTime.Now.AddSeconds(Rand.Next(20, 30));
                    // await message.Channel.SendBaseEmbed(message.Author,Client,"XP выдан",$"ServerId: {serverId}\nUserId:{userId}\nxpgrant:{xpGrant}\ncd:{userL.NewMessage}\ncur:{DateTime.Now}"); // дебагер

                    
                }

            }
        }
    
}