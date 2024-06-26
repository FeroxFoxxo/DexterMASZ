﻿using Bot.Abstractions;
using Bot.Services;
using Punishments.Models;

namespace Punishments.Data;

public class PunishmentConfigRepository(PunishmentDatabase punishmentDatabase,
    DiscordRest discordRest) : Repository(discordRest)
{
    private readonly PunishmentDatabase _punishmentDatabase = punishmentDatabase;

    public async Task<PunishmentConfig> GetGuildPunishmentConfig(ulong guildId) =>
        await _punishmentDatabase.SelectPunishmentConfig(guildId);
}
