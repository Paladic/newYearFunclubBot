using Infrastructure.Context;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataAccessLayer;

public class Users
{
    private readonly IDbContextFactory<DiscordBotDbContext> _contextFactory;

    public Users(IDbContextFactory<DiscordBotDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }
    
    
    public async Task NewUser(ulong id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var user = await context.Users
            .FirstOrDefaultAsync(x => x.Id == id);

        if (user == null)
            context.Add(new User { Id = id});
        await context.SaveChangesAsync();
    }
    
    public async Task<ulong> GetCastleId(ulong id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var user = await context.Users
            .FirstOrDefaultAsync(x => x.Id == id);
        if (user == null)
        {
            await NewUser(id);
            return (ulong) await Task.FromResult(0);
        }

        return await Task.FromResult(user.CastleId);
    }
    
    public async Task<User?> GetUser(ulong id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var user = await context.Users
            .FirstOrDefaultAsync(x => x.Id == id);
        
        if (user == null)
        {
            await NewUser(id);
            user = await context.Users
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        return await Task.FromResult(user);
    } 
    
    public async Task<int> GetSnowball(ulong id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var user = await context.Users
            .FirstOrDefaultAsync(x => x.Id == id);
        
        if (user == null)
        {
            await NewUser(id);
            return await Task.FromResult(0);
        }

        return await Task.FromResult(user.Snowball);
    } 
    
    public async Task<ulong> GetDamageEnd(ulong id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var user = await context.Users
            .FirstOrDefaultAsync(x => x.Id == id);
        
        if (user == null)
        {
            await NewUser(id);
            return (ulong) await Task.FromResult(0);
        }

        return await Task.FromResult(user.DamageEnd);
    }
    
    public async Task ModifyCastleId(ulong id, ulong castleId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
            
        var user = await context.Users
            .FirstOrDefaultAsync(x => x.Id == id);

        if (user == null)
        {
            await NewUser(id);
            user = await context.Users
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        user.CastleId = castleId; 

        await context.SaveChangesAsync();
    }
    
    public async Task ModifySnowball(ulong id, int snowball)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var user = await context.Users
            .FirstOrDefaultAsync(x => x.Id == id);

        if (user == null)
        {
            await NewUser(id);
            user = await context.Users
                .FirstOrDefaultAsync(x => x.Id == id);
        }
        
        user.Snowball += snowball; 

        await context.SaveChangesAsync();
    }
    
    
    public async Task ModifyDamageEnd(ulong id, ulong damageEnd)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var user = await context.Users
            .FirstOrDefaultAsync(x => x.Id == id);

        if (user == null)
        {
            await NewUser(id);
            user = await context.Users
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        user.DamageEnd = damageEnd;
        await context.SaveChangesAsync();
    }


    public async Task<List<User>> GetTeamList(ulong castleId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var nowTime = (ulong) DateTimeOffset.Now.ToUnixTimeSeconds();
        
        var users = await context.Users
            .Where(x => x.CastleId == castleId && x.DamageEnd < nowTime)
            .ToListAsync();

        return await Task.FromResult(users);
    }
}