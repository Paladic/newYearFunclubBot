using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
namespace Infrastructure.Context;

public class DiscordBotDbContextFactory : IDesignTimeDbContextFactory<DiscordBotDbContext>
{
    
    // dotnet ef migrations add name
    // dotnet ef database update
    // Последняя 43
    public DiscordBotDbContext CreateDbContext(string[] args)
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("infsettings.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder()
            .UseMySql(config[$"ConnectionStrings:Default"],
                new MySqlServerVersion(new Version(8, 0, 27)));
            
        return new DiscordBotDbContext(optionsBuilder.Options);
            
    }
    
}