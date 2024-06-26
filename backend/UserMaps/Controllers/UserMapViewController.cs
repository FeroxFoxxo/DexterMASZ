using Bot.Abstractions;
using Bot.Enums;
using Bot.Services;
using Microsoft.AspNetCore.Mvc;
using UserMaps.Data;
using UserMaps.Models;

namespace UserMaps.Controllers;

[Route("api/v1/guilds/{guildId}/usermapview")]
public class UserMapViewController(IdentityManager identityManager, UserMapRepository userMapRepo,
    DiscordRest discordRest) : AuthenticatedController(identityManager, userMapRepo)
{
    private readonly DiscordRest _discordRest = discordRest;
    private readonly UserMapRepository _userMapRepo = userMapRepo;

    [HttpGet]
    public async Task<IActionResult> GetGuildUserNoteView([FromRoute] ulong guildId)
    {
        var identity = await SetupAuthentication();

        await identity.RequirePermission(DiscordPermission.Moderator, guildId);

        var userMaps = await _userMapRepo.GetUserMapsByGuild(guildId);
        List<UserMapExpanded> userMapsViews = [];

        foreach (var userMap in userMaps)
        {
            userMapsViews.Add(new UserMapExpanded(
                userMap,
                await _discordRest.FetchUserInfo(userMap.UserA, true),
                await _discordRest.FetchUserInfo(userMap.UserB, true),
                await _discordRest.FetchUserInfo(userMap.CreatorUserId, true)
            ));
        }

        return Ok(userMapsViews);
    }
}
