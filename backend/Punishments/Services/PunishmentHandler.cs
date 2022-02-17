using Bot.Abstractions;
using Bot.Data;
using Bot.Enums;
using Bot.Events;
using Bot.Exceptions;
using Bot.Models;
using Bot.Services;
using Discord.WebSocket;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Punishments.Data;
using Punishments.Enums;
using Punishments.Models;
using Punishments.Translators;
using Timer = System.Timers.Timer;

namespace Punishments.Services;

public class PunishmentHandler : Event
{
	private readonly BotEventHandler _botEventHandler;
	private readonly DiscordSocketClient _client;
	private readonly DiscordRest _discordRest;
	private readonly ILogger<PunishmentHandler> _logger;
	private readonly IServiceProvider _serviceProvider;

	public PunishmentHandler(BotEventHandler botEventHandler, DiscordRest discordRest,
		ILogger<PunishmentHandler> logger, IServiceProvider serviceProvider, DiscordSocketClient client)
	{
		_botEventHandler = botEventHandler;
		_discordRest = discordRest;
		_logger = logger;
		_serviceProvider = serviceProvider;
		_client = client;
	}

	public void RegisterEvents()
	{
		_botEventHandler.OnBotLaunched += StartLoopingCases;
		_client.UserJoined += HandleUserJoin;
	}

	public async Task StartLoopingCases()
	{
		try
		{
			_logger.LogWarning("Starting case loop.");

			Timer eventTimer = new(TimeSpan.FromMinutes(1).TotalMilliseconds)
			{
				AutoReset = true,
				Enabled = true
			};

			eventTimer.Elapsed += (_, _) => CheckAllCurrentPunishments();

			await Task.Run(() => eventTimer.Start());

			CheckAllCurrentPunishments();

			_logger.LogWarning("Finished case loop.");
		}
		catch (Exception ex)
		{
			_logger.LogCritical(ex, "Something went wrong while starting the punishment handling timer.");
		}
	}

	public async void CheckAllCurrentPunishments()
	{
		using var scope = _serviceProvider.CreateScope();
		var database = scope.ServiceProvider.GetRequiredService<PunishmentDatabase>();
		var cases = await database.SelectAllModCasesWithActivePunishments();

		foreach (var element in cases.Where(element => element.PunishedUntil != null).Where(element => element.PunishedUntil <= DateTime.UtcNow))
		{
			try
			{
				await ModifyPunishment(element, RestAction.Deleted);
			}
			catch (Exception ex)
			{
				_logger.LogCritical(ex, $"Something went wrong while undoing punishment for modcase {element.GuildId}/{element.CaseId} ({element.Id}).");
			}
			element.PunishmentActive = false;
			await database.UpdateModCase(element);
		}
	}

	public async Task ModifyPunishment(ModCase modCase, RestAction action)
	{
		using var scope = _serviceProvider.CreateScope();

		try
		{
			var guildConfig = await scope.ServiceProvider.GetRequiredService<GuildConfigRepository>()
				.GetGuildConfig(modCase.GuildId);

			string reason = null;

			try
			{
				var translator = scope.ServiceProvider.GetRequiredService<Translation>();
				translator.SetLanguage(guildConfig);

				reason = action switch
				{
					RestAction.Created => translator.Get<PunishmentNotificationTranslator>()
						.NotificationDiscordAuditLogPunishmentsExecute(modCase.CaseId, modCase.LastEditedByModId,
							modCase.Title.Truncate(400)),
					RestAction.Deleted => translator.Get<PunishmentNotificationTranslator>()
						.NotificationDiscordAuditLogPunishmentsUndone(modCase.CaseId, modCase.Title.Truncate(400)),
					_ => throw new NotImplementedException()
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex,
					$"Failed to resolve audit log reason string for case {modCase.GuildId}/{modCase.CaseId}");
			}

			switch (modCase.PunishmentType)
			{
				case PunishmentType.Mute:
					switch (action)
					{
						case RestAction.Created:
							_logger.LogInformation($"Mute User {modCase.UserId} in guild {modCase.GuildId}");

							if (!modCase.PunishedUntil.HasValue)
								_logger.LogError($"Failed to mute user due to unknown duration length");

							var muteDuration = modCase.PunishedUntil.Value - DateTime.UtcNow;

							await _discordRest.TimeoutGuildUser(modCase.GuildId, modCase.UserId, muteDuration, reason);

							break;
						case RestAction.Deleted:
							_logger.LogInformation($"Unmute User {modCase.UserId} in guild {modCase.GuildId}");

							await _discordRest.RemoveTimeoutFromGuildUser(modCase.GuildId, modCase.UserId, reason);

							break;
					}

					break;
				case PunishmentType.Ban:
					switch (action)
					{
						case RestAction.Created:
							_logger.LogInformation($"Ban User {modCase.UserId} in guild {modCase.GuildId}.");
							await _discordRest.BanUser(modCase.GuildId, modCase.UserId, reason);
							await _discordRest.GetGuildUserBan(modCase.GuildId, modCase.UserId, CacheBehavior.IgnoreCache);
							break;
						case RestAction.Deleted:
							_logger.LogInformation($"Unban User {modCase.UserId} in guild {modCase.GuildId}.");
							await _discordRest.UnBanUser(modCase.GuildId, modCase.UserId, reason);
							_discordRest.RemoveFromCache(CacheKey.GuildBan(modCase.GuildId, modCase.UserId));
							break;
					}
					break;
				case PunishmentType.Kick:
					switch (action)
					{
						case RestAction.Created:
							_logger.LogInformation($"Kick User {modCase.UserId} in guild {modCase.GuildId}.");
							await _discordRest.KickGuildUser(modCase.GuildId, modCase.UserId, reason);
							break;
					}

					break;
			}
		}
		catch (ResourceNotFoundException)
		{
			_logger.LogError($"Cannot execute punishment in guild {modCase.GuildId} - guild config not found.");
		}
	}

	public async Task HandleUserJoin(SocketGuildUser user)
	{
		using var scope = _serviceProvider.CreateScope();
		var database = scope.ServiceProvider.GetRequiredService<PunishmentDatabase>();

		try
		{
			var guildConfig = await scope.ServiceProvider.GetRequiredService<GuildConfigRepository>()
				.GetGuildConfig(user.Guild.Id);

			var modCases = await database.SelectAllModCasesWithActiveMuteForGuildAndUser(user.Guild.Id, user.Id);

			if (modCases.Count == 0)
				return;

			_logger.LogInformation($"Muted user {user.Id} rejoined guild {user.Guild.Id}");

			await ModifyPunishment(modCases[0], RestAction.Created);
		}
		catch (ResourceNotFoundException)
		{
			_logger.LogInformation($"Cannot execute punishment in guild {user.Guild.Id} - guild config not found.");
		}
	}
}