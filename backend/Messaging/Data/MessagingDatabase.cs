﻿using Bot.Abstractions;
using Messaging.Enums;
using Messaging.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Messaging.Data;

public class MessagingDatabase(DbContextOptions<MessagingDatabase> options) : DataContext<MessagingDatabase>(options), IDataContextCreate
{
    public DbSet<ScheduledMessage> ScheduledMessages { get; set; }

    public static void AddContextToServiceProvider(Action<DbContextOptionsBuilder> optionsAction,
        IServiceCollection serviceCollection) =>
        serviceCollection.AddDbContext<MessagingDatabase>(optionsAction);

    public async Task<ScheduledMessage> GetMessage(int id) =>
        await ScheduledMessages.AsQueryable().Where(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<List<ScheduledMessage>> GetScheduledMessages(ulong guildId, int page = 0) =>
        await ScheduledMessages.AsQueryable().Where(x => x.GuildId == guildId).OrderByDescending(x => x.Id)
            .Skip(page * 20).Take(20).ToListAsync();

    public async Task<List<ScheduledMessage>> GetDueMessages() => await ScheduledMessages.AsQueryable()
        .Where(x => x.Status == ScheduledMessageStatus.Pending && x.ScheduledFor < DateTime.UtcNow).ToListAsync();

    public async Task SaveMessage(ScheduledMessage message)
    {
        ScheduledMessages.Add(message);
        await SaveChangesAsync();
    }

    public async Task UpdateMessage(ScheduledMessage message)
    {
        ScheduledMessages.Update(message);
        await SaveChangesAsync();
    }

    public async Task DeleteMessage(ScheduledMessage message)
    {
        ScheduledMessages.Remove(message);
        await SaveChangesAsync();
    }

    public async Task DeleteMessagesForGuild(ulong guildId)
    {
        var messages = await GetScheduledMessages(guildId);
        ScheduledMessages.RemoveRange(messages);
        await SaveChangesAsync();
    }
}
