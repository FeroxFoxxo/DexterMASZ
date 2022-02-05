﻿using MASZ.Bot.Abstractions;
using MASZ.Bot.Dynamics;
using MASZ.Bot.Models;
using MASZ.Bot.Services;

namespace MASZ.Bot.Data;

public class SettingsRepository : Repository
{
	private readonly BotDatabase _botDatabase;
	private readonly ClientIdContainer _clientIdContainer;

	public SettingsRepository(BotDatabase botDatabase, ClientIdContainer clientIdContainer,
		DiscordRest discordRest) : base(discordRest)
	{
		_botDatabase = botDatabase;
		_clientIdContainer = clientIdContainer;
	}

	public async Task<AppSettings> GetAppSettings()
	{
		return await _botDatabase.GetAppSettings(_clientIdContainer.ClientId);
	}

	public async Task AddAppSetting(AppSettings appSettings)
	{
		await _botDatabase.AddAppSetting(appSettings);
	}

	public async Task UpdateAppSetting(AppSettings updatedAppSettings)
	{
		await _botDatabase.UpdateAppSetting(updatedAppSettings);
	}
}