﻿using AutoMods.Enums;
using AutoMods.Models;
using Bot.Abstractions;
using Bot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AutoMods.Data;

public class AutoModDatabase(DbContextOptions<AutoModDatabase> options) : DataContext<AutoModDatabase>(options), IDataContextCreate
{
    public DbSet<AutoModConfig> AutoModConfigs { get; set; }

    public DbSet<AutoModEvent> AutoModEvents { get; set; }

    public static void AddContextToServiceProvider(Action<DbContextOptionsBuilder> optionsAction,
        IServiceCollection serviceCollection) =>
        serviceCollection.AddDbContext<AutoModDatabase>(optionsAction);

    public async Task<List<AutoModConfig>> SelectAllPunishmentsConfigsForGuild(ulong guildId) =>
        await AutoModConfigs.AsQueryable().Where(x => x.GuildId == guildId).ToListAsync();

    public async Task<AutoModConfig> SelectPunishmentsConfigForGuildAndType(ulong guildId, AutoModType type) =>
        await AutoModConfigs.AsQueryable()
            .FirstOrDefaultAsync(x => x.GuildId == guildId && x.AutoModType == type);

    public async Task PutPunishmentsConfig(AutoModConfig modConfig)
    {
        AutoModConfigs.Update(modConfig);
        await SaveChangesAsync();
    }

    public async Task DeleteSpecificPunishmentsConfig(AutoModConfig modConfig)
    {
        AutoModConfigs.Remove(modConfig);
        await SaveChangesAsync();
    }

    public async Task DeleteAllPunishmentsConfigsForGuild(ulong guildId)
    {
        var events = await AutoModConfigs.AsQueryable().Where(x => x.GuildId == guildId).ToListAsync();
        AutoModConfigs.RemoveRange(events);
        await SaveChangesAsync();
    }

    public async Task<int> CountAllPunishmentsEvents() => await AutoModEvents.AsQueryable().CountAsync();

    public async Task<List<DbCount>> GetPunishmentsCountGraph(ulong guildId, DateTime since) =>
        await AutoModEvents.AsQueryable().Where(x => x.GuildId == guildId && x.CreatedAt > since)
            .GroupBy(x => new { x.CreatedAt.Month, x.CreatedAt.Year })
            .Select(x => new DbCount { Year = x.Key.Year, Month = x.Key.Month, Count = x.Count() })
            .OrderByDescending(x => x.Year).ThenByDescending(x => x.Month).ToListAsync();

    public async Task<List<AutoModTypeSplit>> GetPunishmentsSplitGraph(ulong guildId, DateTime since) =>
        await AutoModEvents.AsQueryable().Where(x => x.GuildId == guildId && x.CreatedAt > since)
            .GroupBy(x => new { Type = x.AutoModType })
            .Select(x => new AutoModTypeSplit { Type = x.Key.Type, Count = x.Count() }).ToListAsync();

    public async Task<int> CountAllPunishmentsEventsForGuild(ulong guildId) =>
        await AutoModEvents.AsQueryable().Where(x => x.GuildId == guildId).CountAsync();

    public async Task<int> CountAllPunishmentsEventsForSpecificUserOnGuild(ulong guildId, ulong userId) =>
        await AutoModEvents.AsQueryable().Where(x => x.GuildId == guildId && x.UserId == userId)
            .CountAsync();

    public async Task<List<AutoModEvent>> SelectAllPunishmentsEventsForSpecificUser(ulong userId) =>
        await AutoModEvents.AsQueryable().Where(x => x.UserId == userId).ToListAsync();

    public async Task<List<AutoModEvent>> SelectAllPunishmentsEventsForSpecificUser(ulong userId, int minutes)
    {
        var since = DateTime.UtcNow.AddMinutes(-minutes);
        return await AutoModEvents.AsQueryable().Where(x => x.UserId == userId && x.CreatedAt > since).ToListAsync();
    }

    public async Task<List<AutoModEvent>> SelectAllPunishmentsEventsForGuild(ulong guildId) =>
        await AutoModEvents.AsQueryable().Where(x => x.GuildId == guildId)
            .OrderByDescending(x => x.CreatedAt).ToListAsync();

    public async Task<List<AutoModEvent>>
        SelectAllPunishmentsEventsForGuild(ulong guildId, int startPage, int pageSize) =>
        await AutoModEvents.AsQueryable().Where(x => x.GuildId == guildId)
            .OrderByDescending(x => x.CreatedAt).Skip(startPage * pageSize).Take(pageSize).ToListAsync();

    public async Task<List<AutoModEvent>> SelectAllPunishmentsEventsForSpecificUserOnGuild(ulong guildId, ulong userId,
        int startPage, int pageSize) =>
        await AutoModEvents.AsQueryable().Where(x => x.GuildId == guildId && x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt).Skip(startPage * pageSize).Take(pageSize).ToListAsync();

    public async Task DeleteAllPunishmentsEventsForGuild(ulong guildId)
    {
        var events = await AutoModEvents.AsQueryable().Where(x => x.GuildId == guildId).ToListAsync();
        AutoModEvents.RemoveRange(events);
        await SaveChangesAsync();
    }

    public async Task SavePunishmentsEvent(AutoModEvent modEvent)
    {
        await AutoModEvents.AddAsync(modEvent);
        await SaveChangesAsync();
    }
}
