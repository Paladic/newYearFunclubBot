using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context;

public class DiscordBotDbContext : DbContext
{
    public DiscordBotDbContext(DbContextOptions options)
        : base(options)
    {
    }
    // dotnet ef migrations add name
    // dotnet ef database update
    
    public DbSet<User> Users { get; set; }
    public DbSet<Castle> Castles { get; set; }
    public DbSet<BlockChannel> BlockChannels { get; set; }
}