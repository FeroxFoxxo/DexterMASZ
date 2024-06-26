using Bot.Abstractions;
using Bot.Data;
using Bot.Exceptions;
using Bot.Extensions;
using Bot.Services;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Invites.Data;
using Invites.Events;
using Invites.Extensions;
using Invites.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Invites.Services;

public class InviteTracker(ILogger<InviteTracker> logger, IServiceProvider serviceProvider, DiscordSocketClient client,
    InviteEventHandler eventHandler) : IEvent
{
    private readonly DiscordSocketClient _client = client;
    private readonly InviteEventHandler _eventHandler = eventHandler;
    private readonly Dictionary<ulong, List<TrackedInvite>> _guildInvites = [];
    private readonly ILogger<InviteTracker> _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public void RegisterEvents()
    {
        _client.GuildAvailable += GuildAvailableHandler;
        _client.GuildUpdated += GuildUpdatedHandler;
        _client.InviteCreated += InviteCreatedHandler;
        _client.UserJoined += GuildUserAddedHandler;
        _client.InviteDeleted += InviteDeletedHandler;
    }

    private Task InviteDeletedHandler(SocketGuildChannel channel, string tracker)
    {
        var invite = RemoveInvite(channel.Guild.Id, tracker).FirstOrDefault();

        if (invite == null)
            return Task.CompletedTask;

        _eventHandler.InviteDeletedEvent.Invoke(channel, invite);

        return Task.CompletedTask;
    }

    private async Task GuildUserAddedHandler(SocketGuildUser user)
    {
        using var scope = _serviceProvider.CreateScope();

        var translator = scope.ServiceProvider.GetRequiredService<Translation>();
        await translator.SetLanguage(user.Guild.Id);

        try
        {
            var guildConfig = await scope.ServiceProvider.GetRequiredService<GuildConfigRepository>()
                .GetGuildConfig(user.Guild.Id);

            if (user.IsBot)
                return;

            var newInvites = await FetchInvites(user.Guild);
            TrackedInvite usedInvite = null;

            try
            {
                usedInvite = GetUsedInvite(user.Guild.Id, newInvites);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get used invite.");
            }

            AddInvites(user.Guild.Id, newInvites);

            if (usedInvite != null)
            {
                UserInvite invite = new()
                {
                    GuildId = user.Guild.Id,
                    JoinedUserId = user.Id,
                    JoinedAt = DateTime.UtcNow,
                    InviteIssuerId = usedInvite.CreatorId,
                    InviteCreatedAt = usedInvite.CreatedAt,
                    TargetChannelId = usedInvite.TargetChannelId,
                    UsedInvite = $"https://discord.gg/{usedInvite.Code}"
                };

                _logger.LogInformation(
                    $"User {user.Username} joined guild {user.Guild.Name} with ID: {user.Guild.Id} using invite {usedInvite.Code}");

                if (guildConfig.ExecuteWhoIsOnJoin)
                {
                    var embed = await invite.CreateInviteEmbed(user, _serviceProvider);

                    await _client.SendEmbed(guildConfig.GuildId, guildConfig.StaffLogs, embed);
                }

                await scope.ServiceProvider.GetRequiredService<InviteRepository>().CreateInvite(invite);
            }
        }
        catch (ResourceNotFoundException)
        {
        }
    }

    private async Task<List<TrackedInvite>> FetchInvites(IGuild guild)
    {
        List<TrackedInvite> invites = [];

        try
        {
            var i = await guild.GetInvitesAsync();
            invites.AddRange(i.Select(x => new TrackedInvite(x, guild.Id)));
        }
        catch (HttpException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to get invites from guild {guild.Id}.");
        }

        try
        {
            var vanityInvite = await guild.GetVanityInviteAsync();
            invites.Add(new TrackedInvite(guild.Id, vanityInvite.Code, vanityInvite.Uses.GetValueOrDefault()));
        }
        catch (HttpException)
        {
        }
        catch (ArgumentNullException)
        { }

        return invites;
    }

    private async Task GuildAvailableHandler(SocketGuild guild) => AddInvites(guild.Id, await FetchInvites(guild));

    private async Task GuildUpdatedHandler(SocketGuild _, SocketGuild newG)
    {
        IInviteMetadata invite = null;

        try
        {
            invite = await newG.GetVanityInviteAsync();
        }
        catch (HttpException)
        {
        }
        catch (ArgumentNullException)
        {
        }

        if (invite != null)
            if (invite.GuildId.HasValue)
                AddInvite(invite.GuildId.Value,
                    new TrackedInvite(invite.GuildId.Value, invite.Code, invite.Uses.GetValueOrDefault()));
    }

    private Task InviteCreatedHandler(SocketInvite invite)
    {
        if (invite.GuildId.HasValue)
            AddInvite(invite.GuildId.Value, new TrackedInvite(invite, invite.GuildId.Value));

        return Task.CompletedTask;
    }

    private TrackedInvite GetUsedInvite(ulong guildId, List<TrackedInvite> currentInvites)
    {
        if (!_guildInvites.TryGetValue(guildId, out var value)) return null;

        var invites = value;

        var changedInvites = invites.Where(x =>
            // Invite is in current invites and has new uses.
            currentInvites.Find(c => c.Code == x.Code) != null &&
            x.HasNewUses(currentInvites.Find(c => c.Code == x.Code)!.Uses) ||
            // Invite is not in current invites and has expired via max uses.
            x.MaxUses.GetValueOrDefault(0) - 1 == x.Uses && currentInvites.Find(c => c.Code == x.Code) == null
        ).ToList();

        if (changedInvites.Count == 1)
            return changedInvites.First();

        var notExpiredInvites = changedInvites.Where(x => !x.IsExpired()).ToList();

        return notExpiredInvites.Count == 1 ? notExpiredInvites.First() : null;
    }

    private void AddInvites(ulong guildId, List<TrackedInvite> invites) => _guildInvites[guildId] = invites;

    private void AddInvite(ulong guildId, TrackedInvite invite)
    {
        if (!_guildInvites.TryGetValue(guildId, out var value))
        {
            _guildInvites[guildId] = [invite];
        }
        else
        {
            var invites = value;
            var filteredInvites = invites.Where(x => x.Code != invite.Code).ToList();
            filteredInvites.Add(invite);
            _guildInvites[guildId] = filteredInvites;
        }
    }

    private List<TrackedInvite> RemoveInvite(ulong guildId, string code)
    {
        if (!_guildInvites.TryGetValue(guildId, out var value))
            return [];

        var invites = value.Where(x => x.Code == code).ToList();
        value.RemoveAll(x => x.Code == code);

        return invites;
    }
}
