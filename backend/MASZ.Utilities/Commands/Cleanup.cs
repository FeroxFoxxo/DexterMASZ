using System.Net;
using Discord;
using Discord.Interactions;
using Discord.Net;
using MASZ.Bot.Abstractions;
using MASZ.Bot.Attributes;
using MASZ.Bot.Enums;
using MASZ.Bot.Translators;
using MASZ.Utilities.Enums;
using MASZ.Utilities.Translators;

namespace MASZ.Utilities.Commands;

public class Cleanup : Command<Cleanup>
{
	[Require(RequireCheck.GuildModerator)]
	[SlashCommand("cleanup", "Cleanup specific data from the server and/or channel.")]
	public async Task CleanupCommand(
		[Summary("mode", "which data you want to delete")]
		CleanupMode cleanupMode,
		[Summary("channel", "where to delete, defaults to current.")]
		ITextChannel channel = null,
		[Summary("count", "how many messages to scan for your mode.")]
		long count = 100,
		[Summary("user", "additional filter on this user")]
		IUser filterUser = null)
	{
		if (channel is null)
			if (Context.Channel is ITextChannel txtChannel)
			{
				channel = txtChannel;
			}
			else
			{
				await Context.Interaction.RespondAsync(Translator.Get<BotTranslator>().OnlyTextChannel(),
					ephemeral: true);
				return;
			}

		await Context.Interaction.RespondAsync("Cleaning channels...", ephemeral: true);

		if (count > 1000)
			count = 1000;

		var func = cleanupMode switch
		{
			CleanupMode.Bots => IsFromBot,
			CleanupMode.Attachments => HasAttachment,
			_ => new Func<IMessage, bool>(_ => true)
		};

		int deleted;

		try
		{
			deleted = await IterateAndDeleteChannels(channel, (int)count, func, Context.User, filterUser);
		}
		catch (HttpException ex)
		{
			if (ex.HttpCode == HttpStatusCode.Forbidden)
				await Context.Interaction.ModifyOriginalResponseAsync(msg =>
					msg.Content = Translator.Get<BotTranslator>().CannotViewOrDeleteInChannel());
			else if (ex.HttpCode == HttpStatusCode.Forbidden)
				await Context.Interaction.ModifyOriginalResponseAsync(msg =>
					msg.Content = Translator.Get<BotTranslator>().CannotFindChannel());

			return;
		}

		await Context.Interaction.ModifyOriginalResponseAsync(msg =>
			msg.Content = Translator.Get<UtilityTranslator>().DeletedMessages(deleted, channel));
	}

	private static bool HasAttachment(IMessage m)
	{
		return m.Attachments.Count > 0;
	}

	private static bool IsFromBot(IMessage m)
	{
		return m.Author.IsBot;
	}

	private static async Task<int> IterateAndDeleteChannels(ITextChannel channel, int limit,
		Func<IMessage, bool> predicate, IUser currentActor, IUser filterUser = null)
	{
		ulong lastId = 0;
		var deleted = 0;

		while (limit > 0)
		{
			var messages = channel.GetMessagesAsync(lastId, Direction.Before, Math.Min(limit, 100));
			var breakAfterDeleteIteration = false;
			List<IMessage> toDelete = new();

			if (messages == null)
				break;

			var count = await messages.CountAsync();

			if (count == 0)
				break;

			if (count < Math.Min(limit, 100))
				breakAfterDeleteIteration = true;

			foreach (var message in await messages.FlattenAsync())
			{
				lastId = message.Id;
				limit--;

				if (filterUser != null && message.Author.Id != filterUser.Id)
					continue;

				if (!predicate(message)) continue;
				deleted++;

				if (message.CreatedAt.UtcDateTime.AddDays(14) > DateTime.UtcNow)
					toDelete.Add(message);
				else
					await message.DeleteAsync();
			}

			switch (toDelete.Count)
			{
				case >= 2:
				{
					RequestOptions options = new()
					{
						AuditLogReason = $"Bulk delete by {currentActor.Username}#{currentActor.Discriminator} ({currentActor.Id})."
					};

					await channel.DeleteMessagesAsync(toDelete, options);
					break;
				}
				case > 0:
					await toDelete[0].DeleteAsync();
					break;
			}

			if (breakAfterDeleteIteration)
				break;
		}

		return deleted;
	}
}