using MASZ.Bot.Abstractions;
using MASZ.Bot.Enums;
using MASZ.Bot.Exceptions;
using MASZ.Bot.Extensions;
using MASZ.Bot.Services;
using MASZ.MOTDs.Events;
using MASZ.MOTDs.Models;

namespace MASZ.MOTDs.Data;

public class MotdRepository : Repository
{
	private readonly MotdEventHandler _eventHandler;
	private readonly MotdDatabase _guildMotdDatabase;

	public MotdRepository(DiscordRest discordRest, MotdDatabase guildMotdDatabase,
		MotdEventHandler eventHandler) : base(discordRest)
	{
		_guildMotdDatabase = guildMotdDatabase;
		_eventHandler = eventHandler;
	}

	public async Task<GuildMotd> GetMotd(ulong guildId)
	{
		var motd = await _guildMotdDatabase.GetMotdForGuild(guildId);

		if (motd == null)
			throw new ResourceNotFoundException();

		return motd;
	}

	public async Task<GuildMotd> CreateOrUpdateMotd(ulong guildId, string content, bool visible)
	{
		var action = RestAction.Updated;
		GuildMotd motd;

		try
		{
			motd = await GetMotd(guildId);
		}
		catch (ResourceNotFoundException)
		{
			motd = new GuildMotd
			{
				GuildId = guildId
			};
			action = RestAction.Created;
		}

		motd.CreatedAt = DateTime.UtcNow;
		motd.UserId = Identity.Id;
		motd.Message = content;
		motd.ShowMotd = visible;

		await _guildMotdDatabase.SaveMotd(motd);

		if (action == RestAction.Created)
			_eventHandler.GuildMotdCreatedEvent.Invoke(motd, Identity);
		else
			_eventHandler.GuildMotdUpdatedEvent.Invoke(motd, Identity);

		return motd;
	}
}