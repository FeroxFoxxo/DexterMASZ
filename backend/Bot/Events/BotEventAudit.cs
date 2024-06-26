﻿using Bot.Abstractions;
using Bot.Enums;
using Bot.Extensions;
using Bot.Identities;
using Bot.Models;
using Bot.Services;

namespace Bot.Events;

public class BotEventAudit(AuditLogger auditLogger, DiscordRest discordRest, BotEventHandler eventHandler) : IEvent
{
    private readonly AuditLogger _auditLogger = auditLogger;
    private readonly DiscordRest _discordRest = discordRest;
    private readonly BotEventHandler _eventHandler = eventHandler;

    public void RegisterEvents()
    {
        _eventHandler.OnGuildUpdated += OnGuildUpdatedAudit;
        _eventHandler.OnGuildDeleted += OnGuildDeletedAudit;
        _eventHandler.OnGuildRegistered += OnGuildRegisteredAudit;

        _eventHandler.OnInternalCachingDone += OnInternalCachingDoneAudit;
        _eventHandler.OnIdentityRegistered += OnIdentityRegisteredAudit;
    }

    private async Task OnIdentityRegisteredAudit(Identity identity)
    {
        if (identity is not DiscordOAuthIdentity dOauth)
            return;

        var currentUser = dOauth.GetCurrentUser();
        var userDefinition = $"`{currentUser.Username}` (`{currentUser.Id}`)";
        await _auditLogger.QueueLog($"{userDefinition} **logged in** using OAuth.");
    }

    private async Task OnGuildDeletedAudit(GuildConfig guildConfig) =>
        await _auditLogger.QueueLog($"**Guild** `{guildConfig.GuildId}` deleted.");

    private async Task OnGuildUpdatedAudit(GuildConfig guildConfig) =>
        await _auditLogger.QueueLog($"**Guild** `{guildConfig.GuildId}` updated.");

    private async Task OnGuildRegisteredAudit(GuildConfig guildConfig, bool importExistingBans) =>
        await _auditLogger.QueueLog($"**Guild** `{guildConfig.GuildId}` registered.");

    private async Task OnInternalCachingDoneAudit(int _, DateTime nextCache) =>
        await _auditLogger.QueueLog($"Internal cache refreshed with `{_discordRest.GetCache().Keys.Count}` entries. " +
                                    $"Next cache refresh {nextCache.ToDiscordTs(DiscordTimestampFormats.RelativeTime)}.",
            true);
}
