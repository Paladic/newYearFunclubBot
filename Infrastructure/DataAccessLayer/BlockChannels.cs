using Infrastructure.Context;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataAccessLayer;

public class BlockChannels
{
    private readonly IDbContextFactory<DiscordBotDbContext> _contextFactory;

    public BlockChannels(IDbContextFactory<DiscordBotDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }
    
    public async Task NewChannel(ulong channelId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var castle = await context.BlockChannels
            .FirstOrDefaultAsync(x => x.Id == channelId);

        if (castle == null)
            context.Add(new BlockChannel() {Id = channelId});
        await context.SaveChangesAsync();
    }
    
    public async Task DeleteChannel(ulong channelId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
            
        var server = await context.BlockChannels
            .FirstOrDefaultAsync(x =>
            x.Id == channelId);
        if (server != null)
        {
            context.BlockChannels.Remove(server);
        }
    }
    
    public async Task<ulong> GetChannel(ulong channelId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var castle = await context.BlockChannels
            .Where(x => x.Id == channelId)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();

        return await Task.FromResult(castle);
    }
}