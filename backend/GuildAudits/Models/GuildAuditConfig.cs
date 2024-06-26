using GuildAudits.DTOs;
using GuildAudits.Enums;
using System.ComponentModel.DataAnnotations;

namespace GuildAudits.Models;

public class GuildAuditConfig
{
    [Key] public int Id { get; set; }
    public ulong GuildId { get; set; }
    public GuildAuditLogEvent GuildAuditLogEvent { get; set; }
    public ulong ChannelId { get; set; }
    public ulong[] PingRoles { get; set; }
    public ulong[] IgnoreRoles { get; set; }
    public ulong[] IgnoreChannels { get; set; }

    public GuildAuditConfig()
    {
    }

    public GuildAuditConfig(GuildAuditConfigForPutDto dto, ulong guildId)
    {
        GuildId = guildId;
        GuildAuditLogEvent = dto.GuildAuditLogEvent;
        ChannelId = dto.ChannelId;
        PingRoles = dto.PingRoles;
        IgnoreRoles = dto.IgnoreRoles;
        IgnoreChannels = dto.IgnoreChannels;
    }
}
