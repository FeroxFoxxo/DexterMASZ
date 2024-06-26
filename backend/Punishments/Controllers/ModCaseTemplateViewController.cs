using Bot.Abstractions;
using Bot.Enums;
using Bot.Services;
using Microsoft.AspNetCore.Mvc;
using Punishments.Data;
using Punishments.Models;

namespace Punishments.Controllers;

[Route("api/v1/templatesview")]
public class ModCaseTemplateViewController(ModCaseTemplateRepository templateRepository, DiscordRest discordRest,
    IdentityManager identityManager) : AuthenticatedController(identityManager, templateRepository)
{
    private readonly DiscordRest _discordRest = discordRest;
    private readonly ModCaseTemplateRepository _templateRepository = templateRepository;

    [HttpGet]
    public async Task<IActionResult> GetTemplatesView([FromQuery] ulong userId = 0)
    {
        var identity = await SetupAuthentication();

        var templates = await _templateRepository.GetTemplatesBasedOnPermissions(identity);
        var templatesView = new List<ModCaseTemplateExpanded>();

        foreach (var template in templates.Where(x => x.UserId == userId || userId == 0))
        {
            templatesView.Add(new ModCaseTemplateExpanded(
                template,
                await _discordRest.FetchUserInfo(template.UserId, true),
                _discordRest.FetchGuildInfo(template.CreatedForGuildId, CacheBehavior.Default)
            ));
        }

        return Ok(templatesView);
    }
}