using AutoMods.Models;
using Discord;
using Discord.WebSocket;

namespace AutoMods.MessageChecks;

public static class SpamCheck
{
    private static readonly Dictionary<ulong, Dictionary<ulong, List<long>>> MsgBoard = [];

    public static bool Check(IMessage message, AutoModConfig config, DiscordSocketClient _)
    {
        if (config.Limit == null)
            return false;

        if (config.TimeLimitMinutes == null)
            return false;

        if (message.Embeds == null)
            return false;

        var guildId = ((ITextChannel)message.Channel).Guild.Id;

        // Sets the guild config in msg_board if it doesn't exist.
        if (!MsgBoard.ContainsKey(guildId))
            MsgBoard[guildId] = [];

        var timestamp = message.CreatedAt.ToUnixTimeSeconds();

        // Filters out messages older than TimeLimitMinutes.
        // Delta is the time minus the TimeLimitMinutes => the time messages older than should be deleted.
        var delta = timestamp - (long)config.TimeLimitMinutes;

        foreach (var userId in MsgBoard[guildId].Keys.ToList())
        {
            var newTimestamps = MsgBoard[guildId][userId].FindAll(x => x > delta);

            if (newTimestamps.Count > 0)
                MsgBoard[guildId][userId] = newTimestamps;
            else
                MsgBoard[guildId].Remove(userId);
        }

        // Add the message to the "msg_board".
        if (!MsgBoard[guildId].TryGetValue(message.Author.Id, out var value))
            MsgBoard[guildId][message.Author.Id] = [timestamp];
        else
            value.Add(timestamp);

        // Counts the number of messages and check them for being too high
        return MsgBoard[guildId][message.Author.Id].Count > config.Limit;
    }
}
