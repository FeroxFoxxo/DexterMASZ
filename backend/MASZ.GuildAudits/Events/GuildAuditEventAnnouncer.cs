﻿using Discord;
using MASZ.Bot.Abstractions;
using MASZ.Bot.Data;
using MASZ.Bot.Enums;
using MASZ.Bot.Services;
using MASZ.GuildAudits.Extensions;
using MASZ.GuildAudits.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MASZ.GuildAudits.Events;

public class GuildAuditEventAnnouncer : Event
{
	private readonly GuildAuditEventHandler _eventHandler;
	private readonly ILogger<GuildAuditEventAnnouncer> _logger;
	private readonly IServiceProvider _serviceProvider;

	public GuildAuditEventAnnouncer(GuildAuditEventHandler eventHandler, ILogger<GuildAuditEventAnnouncer> logger,
		IServiceProvider serviceProvider)
	{
		_eventHandler = eventHandler;
		_logger = logger;
		_serviceProvider = serviceProvider;
	}

	public void RegisterEvents()
	{
		_eventHandler.OnGuildAuditConfigCreated +=
			async (a, b) => await AnnounceGuildAuditLog(a, b, RestAction.Created);

		_eventHandler.OnGuildAuditConfigUpdated +=
			async (a, b) => await AnnounceGuildAuditLog(a, b, RestAction.Updated);

		_eventHandler.OnGuildAuditConfigDeleted +=
			async (a, b) => await AnnounceGuildAuditLog(a, b, RestAction.Deleted);
	}

	private async Task AnnounceGuildAuditLog(GuildAuditConfig config, IUser actor, RestAction action)
	{
		using var scope = _serviceProvider.CreateScope();

		_logger.LogInformation(
			$"Announcing guild audit log {config.GuildId}/{config.GuildAuditLogEvent} ({config.Id}).");

		var guildConfig = await scope.ServiceProvider.GetRequiredService<GuildConfigRepository>()
			.GetGuildConfig(config.GuildId);

		if (!string.IsNullOrEmpty(guildConfig.ModInternalNotificationWebhook))
		{
			_logger.LogInformation(
				$"Sending internal webhook for guild audit log {config.GuildId}/{config.GuildAuditLogEvent} ({config.Id}) to {guildConfig.ModInternalNotificationWebhook}.");

			try
			{
				var embed = await config.CreateGuildAuditEmbed(actor, action, scope.ServiceProvider);
				await DiscordRest.ExecuteWebhook(guildConfig.ModInternalNotificationWebhook, embed.Build());
			}
			catch (Exception e)
			{
				_logger.LogError(e,
					$"Error while announcing guild audit log {config.GuildId}/{config.GuildAuditLogEvent} ({config.Id}) to {guildConfig.ModInternalNotificationWebhook}.");
			}
		}
	}
}