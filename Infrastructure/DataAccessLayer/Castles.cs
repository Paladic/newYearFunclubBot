using Infrastructure.Context;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataAccessLayer;

public class Castles
{
    private readonly IDbContextFactory<DiscordBotDbContext> _contextFactory;

    public Castles(IDbContextFactory<DiscordBotDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }
    
    public async Task NewCastle(string name, ulong roleId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var castle = await context.Castles
            .FirstOrDefaultAsync(x => x.Name == name);

        if (castle == null)
            context.Add(new Castle { Name = name, RoleId = roleId, CastleSize = 100});
        await context.SaveChangesAsync();
    }
    
    public async Task<ulong> GetCastleId(string name, ulong guildId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var castle = await context.Castles
            .FirstOrDefaultAsync(x => x.Name == name);

        if (castle == null)
        {
            await NewCastle(name, guildId);
            castle = await context.Castles
                .FirstOrDefaultAsync(x => x.Name == name);
        }

        return await Task.FromResult(castle.Id);
    }
    
    public async Task<Castle?> GetCastleFromId(ulong id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var castle = await context.Castles
            .FirstOrDefaultAsync(x => x.Id == id);
        
        return await Task.FromResult(castle);
    }
    
    public async Task<List<Castle>> GetCastles()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var castle = await context.Castles
            .ToListAsync();
        
        return await Task.FromResult(castle);
    }
    
    public async Task ModifyCastleSize(ulong id, int count)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var castle = await context.Castles
            .FirstOrDefaultAsync(x => x.Id == id);

        castle.CastleSize += count;
        
        await context.SaveChangesAsync();
    }
    
    public async Task<ulong> GetRoleId(ulong id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var castle = await context.Castles
            .FirstOrDefaultAsync(x => x.Id == id);
        
        return await Task.FromResult(castle.RoleId);
    }
    
    public async Task ModifyRoleId(ulong id, ulong roleId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var castle = await context.Castles
            .FirstOrDefaultAsync(x => x.Id == id);

        castle.RoleId = roleId;
        
        await context.SaveChangesAsync();
    }
    
    public async Task ModifySnowmanCount(ulong id, int count)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var castle = await context.Castles
            .FirstOrDefaultAsync(x => x.Id == id);

        castle.SnowmanCount += count;
        
        await context.SaveChangesAsync();
    }
}