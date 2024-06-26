﻿using Bot.Attributes;
using Discord.Interactions;
using Music.Abstractions;
using Music.Attributes;

namespace Music.Commands;

public class RemoveDuplicatesCommand : MusicCommand<RemoveDuplicatesCommand>
{
    [SlashCommand("remove-duplicates", "Remove duplicating tracks from the list")]
    [BotChannel]
    [QueueNotEmpty]
    public async Task RemoveDuplicates()
    {
        Player.Queue.Distinct();

        await RespondInteraction("Removed duplicating tracks with same source from the queue");
    }
}
