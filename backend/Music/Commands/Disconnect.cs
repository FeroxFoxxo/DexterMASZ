﻿using Bot.Attributes;
using Discord.Interactions;
using Music.Abstractions;

namespace Music.Commands;

public class DisconnectCommand : MusicCommand<DisconnectCommand>
{
    [SlashCommand("disconnect", "Leaves the current voice channel")]
    [BotChannel]
    public async Task Disconnect()
    {
        await Player.DisconnectAsync();

        await RespondInteraction("Left the voice channel");
    }
}
