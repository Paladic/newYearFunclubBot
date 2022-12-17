namespace Infrastructure.Models;

public class User
{
    
    public ulong Id { get; set; } // Айди пользователя. Пример: 219535226462928896
    public ulong CastleId { get; set; }
    public int Snowball { get; set; }
    public ulong DamageEnd { get; set; }
}