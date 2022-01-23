using Discord;
using MASZ.Bot.Abstractions;
using MASZ.Bot.Data;
using MASZ.Bot.Enums;
using MASZ.Bot.Exceptions;
using MASZ.Bot.Models;
using Microsoft.Extensions.DependencyInjection;

namespace MASZ.Bot.Identities;

public class DiscordCommandIdentity : Identity
{
	private readonly Dictionary<ulong, IGuildUser> _guildMemberships = new();

	public DiscordCommandIdentity(IUser currentUser,
		List<UserGuild> userGuilds, IServiceProvider serviceProvider) : base(currentUser.Id.ToString(), serviceProvider)
	{
		CurrentUser = currentUser;
		CurrentUserGuilds = userGuilds;
	}

	public virtual async Task<IGuildUser> GetGuildMembership(ulong guildId)
	{
		if (_guildMemberships.ContainsKey(guildId))
			return _guildMemberships[guildId];

		if (CurrentUser is null)
			return null;

		var guildUser = await DiscordRest.FetchUserInfo(guildId, CurrentUser.Id, CacheBehavior.Default);

		if (guildUser is null)
			return null;

		_guildMemberships[guildId] = guildUser;
		return guildUser;
	}

	public override async Task<bool> HasAdminRoleOnGuild(ulong guildId)
	{
		if (!IsOnGuild(guildId))
			return false;

		try
		{
			using var scope = ServiceProvider.CreateScope();
			var guildConfigRepo = scope.ServiceProvider.GetRequiredService<GuildConfigRepository>();

			var guildConfig = await guildConfigRepo.GetGuildConfig(guildId);

			var guildUser = await GetGuildMembership(guildId);

			if (guildUser is null)
				return false;

			return guildUser.Guild.OwnerId == guildUser.Id || guildUser.RoleIds.Any(x => guildConfig.AdminRoles.Contains(x));
		}
		catch (ResourceNotFoundException)
		{
			return false;
		}
	}

	public override async Task<bool> HasModRoleOrHigherOnGuild(ulong guildId)
	{
		if (!IsOnGuild(guildId))
			return false;

		try
		{
			using var scope = ServiceProvider.CreateScope();
			var guildConfigRepo = scope.ServiceProvider.GetRequiredService<GuildConfigRepository>();

			var guildConfig = await guildConfigRepo.GetGuildConfig(guildId);

			var guildUser = await GetGuildMembership(guildId);

			if (guildUser is null)
				return false;

			if (guildUser.Guild.OwnerId == guildUser.Id)
				return true;

			return guildUser.RoleIds.Any(x => guildConfig.AdminRoles.Contains(x) ||
			                                  guildConfig.ModRoles.Contains(x));
		}
		catch (ResourceNotFoundException)
		{
			return false;
		}
	}
	
	public override bool IsOnGuild(ulong guildId)
	{
		return CurrentUser != null && CurrentUserGuilds.Any(x => x.Id == guildId);
	}

	public override async Task<bool> IsSiteAdmin()
	{
		if (CurrentUser is null)
			return false;

		using var scope = ServiceProvider.CreateScope();

		var config = await scope.ServiceProvider
			.GetRequiredService<SettingsRepository>().GetAppSettings();

		return config.SiteAdmins.Contains(CurrentUser.Id);
	}

	public override void RemoveGuildMembership(ulong guildId)
	{
		CurrentUserGuilds.RemoveAll(x => x.Id == guildId);
		_guildMemberships.Remove(guildId);
	}

	public override void AddGuildMembership(IGuildUser user)
	{
		if (CurrentUserGuilds.All(x => x.Id != user.Guild.Id))
			CurrentUserGuilds.Add(new UserGuild(user));

		_guildMemberships[user.Guild.Id] = user;
	}

	public override void UpdateGuildMembership(IGuildUser user)
	{
		if (CurrentUserGuilds.All(x => x.Id != user.Guild.Id))
			CurrentUserGuilds.Add(new UserGuild(user));

		_guildMemberships[user.Guild.Id] = user;
	}
}