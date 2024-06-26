using Bot.Abstractions;
using Bot.Dynamics;
using Bot.Enums;
using Bot.Exceptions;
using Bot.Extensions;
using Bot.Services;
using GuildAudits.Enums;
using GuildAudits.Events;
using GuildAudits.Exceptions;
using GuildAudits.Models;

namespace GuildAudits.Data;

public class GuildAuditConfigRepository(DiscordRest discordRest, GuildAuditDatabase guildAuditDatabase,
    GuildAuditEventHandler eventHandler) : Repository(discordRest), IDeleteGuildData
{
    private readonly GuildAuditEventHandler _eventHandler = eventHandler;
    private readonly GuildAuditDatabase _guildAuditDatabase = guildAuditDatabase;

    public async Task DeleteGuildData(ulong guildId) =>
        await _guildAuditDatabase.DeleteAllAuditLogConfigsForGuild(guildId);

    public async Task<List<GuildAuditConfig>> GetConfigsByGuild(ulong guildId) =>
        await _guildAuditDatabase.SelectAllAuditLogConfigsForGuild(guildId);

    public async Task<GuildAuditConfig> GetConfigsByGuildAndType(ulong guildId, GuildAuditLogEvent type)
    {
        var config = await _guildAuditDatabase.SelectAuditLogConfigForGuildAndType(guildId, type);

        return config ?? throw new ResourceNotFoundException($"GuildAudit config {guildId}/{type} does not exist.");
    }

    public async Task<GuildAuditConfig> UpdateConfig(GuildAuditConfig newValue)
    {
        if (!Enum.IsDefined(typeof(GuildAuditLogEvent), newValue.GuildAuditLogEvent))
            throw new InvalidAuditLogEventException();

        var action = RestAction.Updated;
        GuildAuditConfig auditLogConfig;

        try
        {
            auditLogConfig = await GetConfigsByGuildAndType(newValue.GuildId, newValue.GuildAuditLogEvent);
        }
        catch (ResourceNotFoundException)
        {
            auditLogConfig = new GuildAuditConfig();
            action = RestAction.Created;
        }

        auditLogConfig.GuildId = newValue.GuildId;
        auditLogConfig.GuildAuditLogEvent = newValue.GuildAuditLogEvent;
        auditLogConfig.ChannelId = newValue.ChannelId;
        auditLogConfig.PingRoles = newValue.PingRoles;
        auditLogConfig.IgnoreChannels = newValue.IgnoreChannels;
        auditLogConfig.IgnoreRoles = newValue.IgnoreRoles;


        await _guildAuditDatabase.PutAuditLogConfig(auditLogConfig);

        if (action == RestAction.Created)
            _eventHandler.GuildAuditConfigCreatedEvent.Invoke(auditLogConfig, Identity);
        else
            _eventHandler.GuildAuditUpdatedEvent.Invoke(auditLogConfig, Identity);

        return auditLogConfig;
    }

    public async Task<GuildAuditConfig> DeleteConfigForGuild(ulong guildId, GuildAuditLogEvent type)
    {
        var config = await GetConfigsByGuildAndType(guildId, type);

        await _guildAuditDatabase.DeleteSpecificAuditLogConfig(config);

        _eventHandler.GuildAuditConfigDeletedEvent.Invoke(config, Identity);

        return config;
    }
}
