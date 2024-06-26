﻿using Bot.Attributes;
using Discord;
using Discord.Interactions;
using Fergun.Interactive;
using Music.Abstractions;
using Music.Attributes;
using Music.Extensions;
using System.Text;

namespace Music.Commands;

public class QueueCommand : MusicCommand<QueueCommand>
{
    public InteractiveService InteractiveService { get; set; }

    [SlashCommand("queue", "Check the queue of current playing session")]
    [BotChannel]
    [QueueNotEmpty]
    public async Task Queue()
    {
        var idx = 0;

        StringBuilder text = new();

        foreach (var queuedTrack in Player.Queue)
        {
            var track = queuedTrack.Track;

            var testStr =
                $"{idx + 1} - {Format.Url($"{Format.Bold(Format.Sanitize(track.Title))} by {Format.Bold(track.Author)}", track.Uri?.AbsoluteUri ?? "https://example.com")}";
            text.AppendLine(testStr);
            ++idx;
        }

        var pages = MusicPages.CreatePagesFromString(text.ToString(), "Song Queue", Color.Gold).ToList();

        if (Player.CurrentTrack != null)
            pages.First().AddField("Current Track", Player.CurrentTrack.Title);

        await RespondInteraction("Sending song queue...");

        await InteractiveService.SendPaginator(pages, Context);
    }
}
