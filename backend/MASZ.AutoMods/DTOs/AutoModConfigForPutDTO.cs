using MASZ.AutoMods.Enums;
using MASZ.Punishments.Enums;
using System.ComponentModel.DataAnnotations;

namespace MASZ.AutoMods.DTOs;

public class AutoModConfigForPutDto
{
	[Required] public AutoModType AutoModType { get; set; }

	[Required] public AutoModAction AutoModAction { get; set; }

	public PunishmentType? PunishmentType { get; set; }
	public int? PunishmentDurationMinutes { get; set; }
	public ulong[] IgnoreChannels { get; set; } = Array.Empty<ulong>();
	public ulong[] IgnoreRoles { get; set; } = Array.Empty<ulong>();
	public int? TimeLimitMinutes { get; set; }
	public int? Limit { get; set; }
	public string CustomWordFilter { get; set; }
	public bool SendDmNotification { get; set; }
	public bool SendPublicNotification { get; set; }
	public AutoModChannelNotificationBehavior ChannelNotificationBehavior { get; set; }
}