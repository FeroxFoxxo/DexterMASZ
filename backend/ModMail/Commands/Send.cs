﻿using Bot.Abstractions;
using Discord.Interactions;
using System.ComponentModel.DataAnnotations;

namespace DexterSlash.Commands.ModeratorCommands;

public class Send : Command<Send>
{
	[SlashCommand("send", "Sends an anonymous message to the moderators, which will not show on the server.")]
	public async Task SendCommand([MaxLength(1250)] string message)
	{
		var guildConfig = await new ConfigRepository(Services).GetGuildConfig(Modules.Modmail, Context.Guild.Id) as ConfigModMail;

		var mailRepo = new ModMailRepository(Services);

		var mail = await mailRepo.CreateModMail(
			message,
			Context.User.Id
		);

		var usrMessage = await (DiscordShardedClient.GetChannel(guildConfig.ModMailChannelID) as ITextChannel).SendMessageAsync(
			embed: CreateEmbed(EmojiEnum.Unknown)
				.WithTitle($"Anonymous Modmail #{mail.TrackerID}")
				.WithDescription(mail.Message)
				.WithFooter($"ID: {mail.TrackerID}")
				.Build()
		);

		await mailRepo.UpdateModMail(mail.TrackerID, usrMessage.Id);

		await CreateEmbed(EmojiEnum.Love)
			.WithTitle("Successfully Sent Modmail")
			.WithDescription($"Haiya! Your message has been sent to the staff team.\n\n" +
				$"Your modmail token is: `{mail.TrackerID}`, which is what the moderators use to reply to you. " +
				$"Only give this out to a moderator if you wish to be identified, " +
                    $"or use the ``/modmail list`` command to find your current sent modmails!\n\n" +
				$"Thank you~! - {Context.Guild.Name} Staff Team <3")
			.WithFooter($"ID: {mail.TrackerID}")
			.SendEmbed(Context.Interaction, true);
	}
}
