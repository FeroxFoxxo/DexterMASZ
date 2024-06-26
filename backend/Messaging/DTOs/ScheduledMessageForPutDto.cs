﻿using System.ComponentModel.DataAnnotations;

namespace Messaging.DTOs;

public class ScheduledMessageForPutDto
{
    [Required(ErrorMessage = "Name field is required")]
    [MaxLength(100)]
    public string Name { get; set; }

    [Required(ErrorMessage = "Content field is required")]
    [MaxLength(4096)]
    public string Content { get; set; }

    [Required(ErrorMessage = "ScheduledFor field is required")]
    public DateTime ScheduledFor { get; set; }

    [Required(ErrorMessage = "ChannelId field is required")]
    public ulong ChannelId { get; set; }
}
