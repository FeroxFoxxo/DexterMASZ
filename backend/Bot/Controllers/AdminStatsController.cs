using Bot.Abstractions;
using Bot.Data;
using Bot.Dynamics;
using Bot.Identities;
using Bot.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Dynamic;

namespace Bot.Controllers;

[Route("api/v1/meta")]
public class AdminStatsController(ILogger<AdminStatsController> logger, ScheduledCacher scheduler,
    SettingsRepository settingsRepository, DiscordRest discordRest, IdentityManager identityManager,
    IServiceProvider serviceProvider, CachedServices cachedServices) : AuthenticatedController(identityManager, settingsRepository)
{
    private readonly CachedServices _cachedServices = cachedServices;
    private readonly DiscordRest _discordRest = discordRest;
    private readonly IdentityManager _identityManager = identityManager;
    private readonly ILogger<AdminStatsController> _logger = logger;
    private readonly ScheduledCacher _scheduler = scheduler;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly SettingsRepository _settingsRepository = settingsRepository;

    [HttpGet("adminStats")]
    public async Task<IActionResult> Status()
    {
        var identity = await SetupAuthentication();

        await identity.RequireSiteAdmin();

        List<string> currentLogins = [];

        foreach (var login in _identityManager.GetCurrentIdentities().OfType<DiscordOAuthIdentity>())
        {
            try
            {
                var user = login.GetCurrentUser();

                currentLogins.Add(user is null ? "Invalid user." : $"{user.Username}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting logged in user.");
                currentLogins.Add("Invalid user.");
            }
        }

        var config = await _settingsRepository.GetAppSettings();

        dynamic adminStats = new ExpandoObject();

        adminStats.loginsInLast15Minutes = currentLogins;
        adminStats.defaultLanguage = config.DefaultLanguage;
        adminStats.nextCache = _scheduler.GetNextCacheSchedule();
        adminStats.cachedDataFromDiscord = _discordRest.GetCache().Keys;

        foreach (var repo in _cachedServices.GetInitializedAuthenticatedClasses<IAddAdminStats>(_serviceProvider,
                     identity))
            await repo.AddAdminStatistics(adminStats);

        return Ok(adminStats);
    }

    [HttpPost("cache")]
    public async Task<IActionResult> TriggerCache()
    {
        var identity = await SetupAuthentication();

        await identity.RequireSiteAdmin();

        Task task = new(() =>
        {
            _identityManager.ClearAllIdentities();
            _scheduler.CacheAll();
        });

        task.Start();

        return Ok();
    }
}
