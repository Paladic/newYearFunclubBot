using Discord.WebSocket;

namespace NewYearBot_Funclub.Utilites;

public class SnowesStorage
{
    
    public class TextSnowes
    {
        public SocketGuildUser User;
        public DateTime NewMessage;
    } 
    public class VoiceSnowes
    {
        public SocketGuildUser User;
        public DateTime ConnectionStart;
        public int StoredSecond;
        public bool Mark;
        public SocketVoiceChannel CurrentVoice;
    }

    public class UserInfo
    {
        public SocketGuildUser User;
        public long AttackCooldown;
        public long PixelCooldown;
    }
}