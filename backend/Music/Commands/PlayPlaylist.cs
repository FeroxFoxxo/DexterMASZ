﻿using Discord;
using Discord.Interactions;
using Fergun.Interactive.Pagination;
using Lavalink4NET.Player;
using Music.Extensions;
using System.Text;

namespace Music.Commands;

public partial class MusicCommand
{
    [SlashCommand("play-playlist", "Add tracks from a playlist to queue")]
    public async Task AddPlaylistMusic(
        [Summary("playlist-url", "Playlist URL")]
        string playlistUrl)
    {
        await Context.Interaction.DeferAsync();

        if (!await EnsureUserInVoiceAsync()) return;
        if (!await EnsureClientInVoiceAsync()) return;

        if (!Uri.IsWellFormedUriString(playlistUrl, UriKind.Absolute))
        {
            await Context.Interaction.ModifyOriginalResponseAsync(x =>
                x.Content = "You need to provide a valid URL");

            return;
        }

        var tracks = await Lavalink.GetTracksAsync(playlistUrl);
        var lavalinkTracks = tracks.ToList();

        if (!lavalinkTracks.Any())
        {
            await Context.Interaction.ModifyOriginalResponseAsync(x =>
                x.Content = "Unable to get the tracks");

            return;
        }

        List<LavalinkTrack> postProcessedTracks = new();

        StringBuilder text = new();
        var idx = 0;

        foreach (var track in lavalinkTracks)
        {
            var testStr = track.IsLiveStream
                ? $"Live stream skipped: {Format.Url($"{Format.Bold(track.Title)} by {Format.Bold(track.Author)}", track.Uri?.AbsoluteUri ?? "https://example.com")}"
                : $"{idx + 1} - {Format.Url($"{Format.Bold(Format.Sanitize(track.Title))} by {Format.Bold(track.Author)}", track.Uri?.AbsoluteUri ?? "https://example.com")}";

            text.AppendLine(testStr);

            if (track.IsLiveStream) continue;

            ++idx;
            postProcessedTracks.Add(track);
        }

        _player!.Queue.AddRange(postProcessedTracks);

        var pages = MusicPages.CreatePagesFromString($"{text}");

        var paginator = new StaticPaginatorBuilder()
            .AddUser(Context.User)
            .WithPages(pages)
            .Build();

        await InteractiveService.SendPaginatorAsync(paginator, Context.Interaction,
            responseType: InteractionResponseType.DeferredChannelMessageWithSource);
    }
}
