using MASZ.Bot.Enums;
using System.ComponentModel.DataAnnotations;

namespace MASZ.Bot.DTOs;

public class GuildConfigForCreateDto
{
	[Required(ErrorMessage = "GuildId field is required")]
	public ulong GuildId { get; set; }

	[Required(ErrorMessage = "ModRoles field is required")]
	public ulong[] ModRoles { get; set; }

	[Required(ErrorMessage = "AdminRoles field is required")]
	public ulong[] AdminRoles { get; set; }

	[Required(ErrorMessage = "MutedRoles field is required")]
	public ulong[] MutedRoles { get; set; }

	public bool ModNotificationDm { get; set; }

	[Url(ErrorMessage = "Webhook needs to be a valid url")]
	[RegularExpression(@"https://(.*\.)?discord(app)?.com/api/webhooks/[0-9]+/.+", ErrorMessage = "Must be a valid webhook url")]
	public string ModPublicNotificationWebhook { get; set; }

	[Url(ErrorMessage = "Webhook needs to be a valid url")]
	[RegularExpression(@"https://(.*\.)?discord(app)?.com/api/webhooks/[0-9]+/.+", ErrorMessage = "Must be a valid webhook url")]
	public string ModInternalNotificationWebhook { get; set; }

	[Required(ErrorMessage = "ExecuteWhoIsOnJoin field is required")]
	public bool ExecuteWhoIsOnJoin { get; set; }

	[Required(ErrorMessage = "StrictModPermissionCheck field is required")]
	public bool StrictModPermissionCheck { get; set; }

	[Required(ErrorMessage = "PublishModeratorInfo field is required")]
	public bool PublishModeratorInfo { get; set; }

	public Language? PreferredLanguage { get; set; } = null;
}